<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UploadBoxV1.ascx.cs" Inherits="IOER.Controls.UploadBoxV1" %>

<script type="text/javascript">
	//Hold information for individual boxes
	if (typeof (uploadBoxes) == "undefined") {
		var uploadBoxes = [];
	}
	if (typeof (uploaders) == "undefined") {
		var uploaders = {};
		uploaders.uploadID = 0;
	}

	//Initialize
	$(document).ready(function () {
		uploaders.setupUploadBoxes();
	});

	//Setup uninitialized boxes
	uploaders.setupUploadBoxes = function() {
		$(".uploadBox").not(".initialized").each(function () {
			var box = $(this);
			var boxID = box.attr("data-boxID");
			box.addClass("initialized");
			uploadBoxes.push(
				{
					boxID: boxID,
					maxSize: parseInt(box.attr("data-maxSize")),
					multiple: box.attr("data-multiple") == "true",
					accept: box.attr("data-accept"),
					box: box,
					input: box.find("input[type=file]"),
					selectedFiles: [],
					gdPicker: null,
					gdSelectedFiles: []
				}
			);
			box.find("input[type=file]").on("change", function() {
				var uploader = uploaders.getUploadBox(boxID)
				uploader.selectedFiles = uploaders.getSelectedFiles(boxID);
				//If not multiple, wipe selected google files
				if(!uploader.multiple && uploader.selectedFiles.length > 0){
					uploader.gdSelectedFiles = [];
				}
				uploaders.countFiles(boxID);
				uploaders.renderFiles(boxID);
			});
		});
	}

</script>
<script type="text/javascript">
	//Page functions
	//Get upload box
	uploaders.getUploadBox = function (id) {
		for (var i in uploadBoxes) {
			if (uploadBoxes[i].boxID == id) {
				return uploadBoxes[i];
			}
		}
	}

	//Get files from upload box
	uploaders.getSelectedFiles = function (id) {
		var box = uploaders.getUploadBox(id).input[0].files;
		var result = [];
		for(var i = 0; i < box.length; i++){
			result.push(box[i]);
		}
		return result;
	}

	//Get current uploads for a box
	uploaders.getUploadsFromBox = function (id) {
		return uploaders.getUploadBox(id).uploads;
	}

	//Validate files
	uploaders.validateFiles = function (id) {
		var box = uploaders.getUploadBox(id);
		var pcFiles = uploaders.getSelectedFiles(id);
		var gdFiles = uploaders.gdGetSelectedFiles(id);
		var errors = [];
		for (var i in pcFiles) {
			var file = pcFiles[i];
			//Validate size
			if (file.size > box.maxSize) {
				errors.push(file.name + " is too large.");
			}
		}

		for(var i in gdFiles){
			var file = gdFiles[i];
			if(file.fileSize > box.maxSize){
				errors.push(file.name + " is too large.");
			}
		}

		return { valid: errors.length == 0, errors: errors };
	}


	//Upload files
	uploaders.uploadFiles = function (id, method, context, callback, button) {
		var box = uploaders.getUploadBox(id);

		//Validate files
		var validation = uploaders.validateFiles(id);
		if (!validation.valid) {
			callback({
				data: null,
				valid: false,
				status: validation.errors.join(". "),
				extra: validation.errors
			});
			return;
		}

		//Append data to context
		context.gdAccessToken = uploaders.gdOauthToken;

		//Create container
		var formData = new FormData();
		formData.append("Context", JSON.stringify(context));

		//Add files
		var files = uploaders.getSelectedFiles(id);
		for(var i in files){
			formData.append("File", files[i], files[i].name);
		}
		
		var gdFileData = [];
		var gdFiles = uploaders.gdGetSelectedFiles(id);
		for(var i in gdFiles){
			var item = gdFiles[i];
			gdFileData.push({
				Id: item.id,
				Name: item.name,
				MimeType: item.mimeType,
				Url: $(".uploadWorkspace[data-boxID=" + id + "] .workspaceUploadItem[data-fileID='" + item.id + "'] .mimeTypeSelector option:selected").attr("data-url")
			});
		}

		formData.append("GoogleDriveFilesData", JSON.stringify(gdFileData));

		if(button){
			button.attr("originalValue", button.attr("value")).attr("value", "...").prop("disabled", true);
		}

		$.ajax({
			url: "/services/AjaxUploadService.asmx/" + method,
			type: "POST",
			data: formData,
			enctype: "multipart/form-data",
			accepts: "application/json",
			processData: false,
			contentType: false
		}).done(function (msg) {
			console.log("Response:", msg);
			var data;
			if (typeof (msg) == "object") { //If the response was XML, convert to JSON
				data = $.parseJSON($(msg).text());
			}
			else if (typeof (msg) == "string") {
				data = $.parseJSON(msg);
			}
			console.log("Parsed data:", data);
			callback(data);

			//Clear selections
			box.gdSelectedFiles = [];
			$(".uploadBox[data-boxID=" + id + "] input[type=file]").val("").trigger("change");
			uploaders.countFiles(id);
			uploaders.renderFiles(id);

			if(button){
				button.attr("value", button.attr("originalValue")).prop("disabled", false);
			}

		});
	}

	//Set status
	uploaders.setUploadStatus = function(id, message, mode){
		var box = $(".uploadBox[data-boxID=" + id + "] .uploadStatus")
		if(message){
			box.html(message);
		}
		if(mode){
			box.attr("data-mode", mode);
		}
	}

	//Count total selected files
	uploaders.countFiles = function(id) {
		var box = $(".uploadBox[data-boxID=" + id + "] .uploadStatus");
		var uploader = uploaders.getUploadBox(id);
		var selectedFiles = uploader.selectedFiles;
		var selectedGDFiles = uploader.gdSelectedFiles;
		if(selectedFiles.length == 0 && selectedGDFiles.length == 0){
			uploaders.setUploadStatus(id, "No files selected");
		}
		else {
			var first = selectedFiles.length > 0 ? ( selectedFiles.length + " file(s) from your device" ) : "";
			var second = selectedGDFiles.length > 0 ? ( selectedGDFiles.length + " file(s) from Google Drive" ) : "";
			var and = selectedFiles.length > 0 && selectedGDFiles.length > 0 ? " and " : "";
			uploaders.setUploadStatus(id, "Selected " + first + and + second + ".");
		}
	}
</script>
<script type="text/javascript">
	/* Google Drive Mime Types */
	uploaders.mimeTypes = {
		//Document
		"application/pdf": "PDF Document (.pdf)",
		"application/rtf": "Rich Text Format (.rtf)",
		"application/vnd.openxmlformats-officedocument.wordprocessingml.document": "Microsoft Word (.docx)",
		"application/vnd.oasis.opendocument.text": "OpenDocument Text (.odt)",
		"application/x-vnd.oasis.opendocument.text": "OpenDocument Text (.odt)",
		"text/html": "Web Page (.html, zipped)",
		"text/plain": "Plain Text (.txt)",
		"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "Microsoft Excel (.xlsx)",
		"application/vnd.oasis.opendocument.spreadsheet": "OpenDocument Spreadsheet (.ods)",
		"application/x-vnd.oasis.opendocument.spreadsheet": "OpenDocument Spreadsheet (.ods)",
		"text/csv": "Comma-Separated Values (.csv)",
		"application/vnd.openxmlformats-officedocument.presentationml.presentation": "Microsoft PowerPoint (.pptx)",
		"application/vnd.oasis.opendocument.presentation": "OpenDocument Presentation (.odp)",
		"application/x-vnd.oasis.opendocument.presentation": "OpenDocument Presentation (.odp)",
		//Image
		"image/jpeg": "JPEG image (.jpg)",
		"image/jpe": "JPEG image (.jpg)",
		"image/jpg": "JPEG image (.jpg)",
		"image/png": "PNG image (.png)",
		"image/gif": "GIF image (.gif)",
		"image/svg+xml": "SVG image (.svg)",
		"image/bmp": "Bitmap image (.bmp)",
		"image/tif": "TIF image (.tif)",
		"image/tiff": "TIF image (.tif)",
		//Audio
		"audio/m4a": "M4A audio (.m4a)",
		"audio/mid": "MIDI audio (.midi)",
		"audio/midi": "MIDI audio (.midi)",
		"audio/mpeg": "MP3 audio (.mp3)",
		"audio/wav": "WAV audio (.wav)",
		"audio/x-ms-wma": "WMA audio (.wma)",
		//Video
		"video/x-msvideo": "AVI video (.avi)",
		"video/mpeg": "MPEG video (.mpg)",
		"video/mpg": "MPEG video (.mpg)",
		"video/mp4": "MP4 video (.mp4)",
		"video/quicktime": "MOV video (.mov)",
		"video/x-ms-wmv": "WMV video (.wmv)",
		"video/3gp": "3GP video (.3gp)",
		"video/3gpp": "3GP video (.3gp)",
		"video/3gpp2": "3GP video (.3gp)",
		//Archive
		"application/x-zip-compressed": "ZIP archive (.zip)",
		"application/zip": "ZIP archive (.zip)",
		"application/x-7z-compressed": "7Z archive (.7z)",
		"application/x-rar-compressed": "RAR archive (.rar)"
	};
	uploaders.getMimeTypeTitle = function(mimeType){
		for(var i in uploaders.mimeTypes){
			if(i == mimeType){
				return uploaders.mimeTypes[i];
			}
		}
		return "Unknown File Type (" + mimeType + ")";
	}
</script>
<script type="text/javascript">
	/* Google Drive related items */
	if (typeof (uploaders.gdExists) == "undefined") {
		uploaders.gdExists = false;
		uploaders.gdSelectedFiles = [];
	}
	uploaders.gdClientID = "<%=GoogleDriveClientId %>";
	uploaders.gdPickerApiLoaded = false;
	uploaders.gdDriveApiLoaded = false;
	uploaders.gdOauthToken = "";
	uploaders.gdActivePickerID = "";

	//API asynchronous load functions
	function uploaders_gdOnApiLoad() { //Can't use dot in URL parameter based onload
		uploaders.gdOnApiLoad();
	} 
	uploaders.gdOnApiLoad = function () {
		//Ensure API stuff is only loaded once per page, even if there are multiple UploadBox controls
		if (typeof (uploaders.gdExists) == "undefined" || uploaders.gdExists == false) {
			uploaders.gdExists = true;
			gapi.load("auth", { "callback": uploaders.gdOnAuthApiLoad });
			gapi.load("picker", { "callback": uploaders.gdOnPickerApiLoad });
		}
	}
	uploaders.gdOnAuthApiLoad = function () {
		uploaders.gdOauthToken = gapi.auth.getToken();
		if(uploaders.gdOauthToken == null){ 
			console.log("Reauthorizing Google API...");
			gapi.auth.authorize(
			{
				client_id: uploaders.gdClientID,
				scope: "https://www.googleapis.com/auth/drive.readonly",
				immediate: true
			},
			uploaders.gdOnApiAuthorize);
		}
		else {
			console.log("Already have Google API token: " + uploaders.gdOauthToken);
			uploaders.gdOnApiAuthorize({ access_token: uploaders.gdOauthToken });
		}
	}
	uploaders.gdOnApiAuthorize = function (result) {
		if (result && !result.error) {
			uploaders.gdOauthToken = result.access_token;
			console.log("Obtained Google API token: " + result.access_token);
			gapi.load("client", { "callback": uploaders.gdOnClientApiLoad });
			uploaders.gdCreatePickers();
		}
		else if (result) {
			console.log("Error authorizing google drive API:", result.error)
		}
		else {
			console.log("Error authorizing google drive API: No result received!");
			console.log(result);
		}
	}
	uploaders.gdOnPickerApiLoad = function () {
		uploaders.gdPickerApiLoaded = true;
		uploaders.gdCreatePickers();
	}
	uploaders.gdOnClientApiLoad = function() {
		gapi.client.load("drive", "v2", uploaders.gdOnDriveApiLoad );
	}
	uploaders.gdOnDriveApiLoad = function() {
		uploaders.gdDriveApiLoaded = true;
		console.log("drive loaded");
		uploaders.gdCreatePickers();
	}
	//Create all pickers
	uploaders.gdCreatePickers = function () {
		if (uploaders.gdPickerApiLoaded && uploaders.gdOauthToken != "" && uploaders.gdDriveApiLoaded) {
			console.log("Creating pickers");
			for (var i = 0; i < uploadBoxes.length; i++) {
				if (uploadBoxes[i].gdPicker == null) {
					uploadBoxes[i].gdPicker = uploaders.gdCreatePicker(uploadBoxes[i].boxID);
				}
			}
			console.log("Unlocking buttons");
			$(".btnSelectGDFile").attr("value", "Select from Google Drive").prop("disabled", false);
		}
	}

	//Create picker
	uploaders.gdCreatePicker = function(id){
		var box = uploaders.getUploadBox(id);
		var picker = new google.picker.PickerBuilder().
			enableFeature(box.multiple ? google.picker.Feature.MULTISELECT_ENABLED : null).
			addView(google.picker.ViewId.DOCS).
			setAppId(uploaders.gdClientID).
			setOAuthToken(uploaders.gdOauthToken).
			setCallback(function (data) { uploaders.gdActivePickerID = id; uploaders.gdPickerCallback(data); }).
			build();
		picker.uploadBoxID = id;
		return picker;
	}
	
	//This gets called when the user clicks "select" in the picker
	uploaders.gdPickerCallback = function (data) {
		var box = uploaders.getUploadBox(uploaders.gdActivePickerID);
		box.gdSelectedFiles = [];
		if (data[google.picker.Response.ACTION] == google.picker.Action.PICKED) {
			var docs = data[google.picker.Response.DOCUMENTS];
			for (var i = 0; i < docs.length; i++) {
				uploaders.gdAddSelectedFile(uploaders.gdActivePickerID, docs[i]);
			}
		}
		uploaders.countFiles(uploaders.gdActivePickerID);
		uploaders.renderFiles(uploaders.gdActivePickerID);
	}

	//Get the links for mime types of a GD file, then add it to the list of selected  GD files
	uploaders.gdAddSelectedFile = function(id, doc) {
		//Get the box
		var box = uploaders.getUploadBox(id);
		//Get the export links or webcontentlink for the file
		gapi.client.drive.files.get({
			fileId: doc.id
		}).execute(function(response){
			//Determine what kind of link(s) to add, then do it
			console.log("Google Drive File response", response);
			doc.links = [];
			doc.fileSize = response.fileSize;
			if(response.exportLinks){
				for(var i in response.exportLinks){
					doc.links.push({ title: uploaders.getMimeTypeTitle(i), type: i, link: response.exportLinks[i] });
				}
			}
			else {
				doc.links.push({ title: uploaders.getMimeTypeTitle(response.mimeType), type: response.mimeType, link: response.downloadUrl });
			}
			//Add the result to the list
			box.gdSelectedFiles.push(doc);
			//Update the list of selected files
			//If not multiple, wipe selected device files
			var uploader = uploaders.getUploadBox(id);
			if(!uploader.multiple){
				$(".uploadBox[data-boxID=" + id + "] input[type=file]").val("").trigger("change");
			}
			uploaders.countFiles(id);
			uploaders.renderFiles(id);
		});
	}

	//Get selected GD files
	uploaders.gdGetSelectedFiles = function (id) {
		return uploaders.getUploadBox(id).gdSelectedFiles;
	}

	//Show the picker
	uploaders.gdShowPicker = function(id){
		uploaders.getUploadBox(id).gdPicker.setVisible(true);
	}

</script>
<script type="text/javascript">
	/* Rendering Functions */

	//Render selected files, enabling the user to select mime types for GD files
	uploaders.renderFiles = function(id) {
		var pcBox = $(".uploadBox .uploadWorkspace[data-boxID=" + id + "] .pcFiles");
		var gdBox = $(".uploadBox .uploadWorkspace[data-boxID=" + id + "] .gdFiles");
		var pcLabel = $(".uploadBox .uploadWorkspace[data-boxID=" + id + "] .pcFilesLabel");
		var gdLabel = $(".uploadBox .uploadWorkspace[data-boxID=" + id + "] .gdFilesLabel");
		var template = $(".uploadItemTemplate[data-boxID=" + id + "]").html();
		var pcFiles = uploaders.getSelectedFiles(id);
		var gdFiles = uploaders.gdGetSelectedFiles(id);
		pcBox.html("");
		for(var i in pcFiles){
			pcBox.append(template
				.replace(/{fileID}/g, uploaders.uploadID)
				.replace(/{mimeTypeSelectOptions}/g, "<option value=\"" + pcFiles[i].type + "\">" + uploaders.getMimeTypeTitle(pcFiles[i].type) + "</option>")
				.replace(/{disabled}/g, "disabled=\"disabled\"")
				.replace(/{title}/g, pcFiles[i].name)
			);
			uploaders.uploadID++;
		}
		//Flag all existing GD items as deleteable
		gdBox.find(".workspaceUploadItem").addClass("deleteMe");
		for(var i in gdFiles){
			//For each newly-selected file, see if there is a matching item in the interface
			var existing = gdBox.find("[data-fileID='" + gdFiles[i].id + "']");
			//If there isn't one, add it
			if(existing.length == 0){
				//Render the list of applicable mime types
				var options = "";
				for(var j in gdFiles[i].links){
					options += "<option value=\"" + gdFiles[i].links[j].type + "\" data-url=\"" + gdFiles[i].links[j].link + "\">" + gdFiles[i].links[j].title + "</option>";
				}
				//Add the item to the box
				gdBox.append(template
					.replace(/{fileID}/g, gdFiles[i].id)
					.replace(/{mimeTypeSelectOptions}/g, options)
					.replace(/{title}/g, gdFiles[i].name)
					.replace(/{disabled}/g, gdFiles[i].links.length == 1 ? "disabled=\"disabled\"" : "")
				);
			}
			//If there is one, remove its delete flag
			else {
				existing.removeClass("deleteMe");
			}
		}
		//Remove anything that still has a delete flag
		gdBox.find(".workspaceUploadItem.deleteMe").remove();

		//Show and hide labels
		if(pcFiles.length > 0){ pcLabel.show(); }
		else { pcLabel.hide(); }
		if(gdFiles.length > 0){ gdLabel.show(); }
		else { gdLabel.hide(); }
	}

</script>
<script type="text/javascript" src="//apis.google.com/js/api.js?onload=uploaders_gdOnApiLoad"></script>
<style type="text/css">
	.uploadBox { font-size: 0; border: 1px solid #CCC; border-radius: 5px; padding: 1px; }
	.uploadBox * { padding: 2px 5px; transition: color 0.2s, background-color 0.2s; }
	.uploadBox input[type=button] { margin-bottom: 1px; }
	.uploadBox input[type=button]:hover, .uploadBox input[type=button]:focus { cursor: pointer; background-color: #FF6A00; color: #FFF; }
	.uploadBox[data-renderMode=block] * { display: block; width: 100%; }
	.uploadBox[data-renderMode=inline] * { display: inline-block; width: 33%; }
	.uploadBox .workspaceUploadItem { border: 1px solid #CCC; padding: 5px; }
	.uploadBox .workspaceUploadItem input, .uploadBox .workspaceUploadItem select { margin: 1px 0; }
</style>
<div class="uploadBox" data-renderMode="<%=RenderMode %>" data-boxID="<%=ClientBoxId %>" data-maxSize="<%=MaxFileSizeInBytes %>" data-multiple="<%=(AllowMultiple ? "true" : "false") %>" data-accept="<%=Accept %>">
	<input type="file" class="isleButton bgBlue btnSelectDeviceFile" <%=(AllowMultiple ? "multiple=\"multiple\"" : "") %> <%=( string.IsNullOrWhiteSpace( Accept ) ? "" : "accept=\"" + Accept + "\"" ) %> />
	<input type="button" class="isleButton bgBlue btnSelectGDFile" value="Google Drive Loading..." disabled="disabled" onclick="uploaders.gdShowPicker('<%=ClientBoxId %>');" />
	<div class="uploadStatus" data-boxID="<%=ClientBoxId %>">No Files Selected</div>
	<div class="uploadWorkspace" data-boxID="<%=ClientBoxId %>">
		<label class="pcFilesLabel" style="display:none;"><b>Files from your device:</b></label>
		<div class="pcFiles"></div>
		<label class="gdFilesLabel" style="display:none;"><b>Files from Google Drive:</b></label>
		<div class="gdFiles"></div>
	</div>
</div>
<script type="text/template" class="uploadItemTemplate" data-boxID="<%=ClientBoxId %>">
	<div class="workspaceUploadItem" data-fileID="{fileID}">
		<div class="fileData">
			<label>Title</label>
			<input type="text" data-field="title" value="{title}" />
		</div><!--
		--><div class="mimeType">
			<label>Upload as</label>
			<select class="mimeTypeSelector" {disabled}>
				{mimeTypeSelectOptions}
			</select>
		</div>
	</div>
</script>