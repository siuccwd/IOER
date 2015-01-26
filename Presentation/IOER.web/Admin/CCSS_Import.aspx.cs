using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using ILPathways.Business;
using LRWarehouse.DAL;
using LWB = LRWarehouse.Business;
using MyManager = LRWarehouse.DAL.StandardDataManager;

using ILPathways.Utilities;

namespace ILPathways.Admin
{
    public partial class CCSS_Import : System.Web.UI.Page
    {
        public int currentSubjectId = 0;
        public int parentId = 1;
        public int standardId = 10;
        public int maxTopics = 0;   //debugging
        public int debugLevel = 5;
        public int STANDARD_BODY_CCSS_ID = 1;
        public int STANDARD_BODY_ASN_ID = 2;


        MyManager myManager = new MyManager();

        protected void Page_Load( object sender, EventArgs e )
        {
            //hard code target xml
            //need to add standard body, and join table keys
            //ProcessFile( this.mathXml.Text );

        }

        public void submitButton_Click( object sender, EventArgs e )
        {
            if ( lstForm.SelectedIndex < 1 )
            {

                lblMessage.Text = "<b>ERROR: A STANDARD BODY MUST BE SELECTED</b>";
                return;

            }

            if ( ddlStandardsFile.SelectedIndex < 1 && txtStandardsFile.Text.Length < 10 )
            {
                lblMessage.Text = "<b>ERROR: Select a standard OR enter a file path (absolute or relative to app_data. ex: /App_Data/D2589605.xml)</b>";
                return;

            } else if ( ddlStandardsFile.SelectedIndex > 0  && txtStandardsFile.Text.Length > 10 )
            {
                lblMessage.Text = "<b>ERROR: Select EITHER a standard OR enter a file path, not both</b>";
                return;

            }
            string file = "";
            if ( ddlStandardsFile.SelectedIndex > 0 )
                file = ddlStandardsFile.SelectedItem.Text;
            else
                file = txtStandardsFile.Text;

            if ( file.ToLower().StartsWith("/") || file.ToLower().StartsWith("app_data"))
            {
                //map path to ?
                file = Server.MapPath( file );
            }

            ProcessFile( file );
        }

        public void ProcessFile( string file )
        {
            LoggingHelper.DoTrace( 1, string.Format( "####Processing file: {0}", file) );
            int topicCount = 0;
            string statusMessage = "";
            XmlDocument standardsDoc = new XmlDocument();
            standardsDoc.Load( file );
            int standardBodyId = int.Parse( this.lstForm.SelectedValue.ToString() );

            //get starting node, with main description
            XmlNodeList records = standardsDoc.GetElementsByTagName( "asn:StandardDocument" );

            foreach ( XmlNode record in records )
            {
                LWB.StandardSubject subject = new LWB.StandardSubject();

                XmlDocument standard = new XmlDocument();
                standard.LoadXml( record.OuterXml );
                string title = GetNodeChildNodeText( record, "dc:title" );

                //check if subject already exists
                subject = myManager.StandardSubject_Get( standardBodyId, title );
                if ( subject == null || subject.Id == 0 )
                {
                    subject.Title = GetNodeChildNodeText( record, "dc:title" );
                    subject.Description = GetNodeChildNodeText( record, "dcterms:description" );
                    subject.Url = GetNodeChildNodeText( record, "dcterms:source" );
                    subject.Language = GetNodeChildNodeText( record, "dcterms:language" );
                    subject.StandardBodyId = standardBodyId;
                    //note need to persist but also handle reruns! - get by standardBodyId & title to see if exists

                    int subjectId = myManager.StandardSubject_Create( subject, ref statusMessage );
                    //TODO - check if OK
                }

                LoggingHelper.DoTrace( 1, string.Format( "===== Subject: {0}\r\n {1}", subject.Title, subject.Description ), false );

                //get all gemq:hasChild - links to the top level or standard domain
                //ex:  <gemq:hasChildX rdf:resource="http://asn.jesandco.org/resources/S2366905"/>
                XmlNodeList domains = standard.GetElementsByTagName( "gemq:hasChild" );
                foreach ( XmlNode domain in domains )
                {

                    string next = GetAttribute( domain, "rdf:resource" );
                    if ( next.Length > 0 )
                    {
                        topicCount++;
                        ProcessDomain( subject.Id, standardsDoc, next );
                    }

                    if ( maxTopics  > 0 && topicCount >= maxTopics )
                        break;
                } //foreach record

            } //foreach record

            this.lblMessage.Text = string.Format("processed topics: {0}", topicCount);
        }

        /// <summary>
        /// process domain
        /// - get all statements and check rdf:about for domain name
        /// - on find
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="topic"></param>
        public void ProcessDomain( int subjectId, XmlDocument doc, string domain )
        {
            currentSubjectId = subjectId;
            XmlNodeList statements = doc.GetElementsByTagName( "asn:Statement" );
            foreach ( XmlNode statement in statements )
            {
                string next = GetAttribute( statement, "rdf:about" );
                ///check if current about matches domain parameter, process if true
                if ( next == domain )
                {
                    //found, 
                    ArrayList levelsCategory = new ArrayList();
                    //handle the domain level 
                    parentId = 0;
                    int domainId = HandleLevel( doc, statement, next, parentId, ref levelsCategory );

                    break;
                }


            } //foreach record
        }

        public int HandleLevel( XmlDocument doc, XmlNode statement, string url, int parentKey, ref ArrayList levelsCategory )
        {
            // may need to pass in a parent key
            XmlDocument level = new XmlDocument();
            level.LoadXml( statement.OuterXml);
            string statusMessage = "";
            int newLevelId = 0;
            XmlNodeList children = level.GetElementsByTagName( "gemq:hasChild" );
            if ( children.Count > 0 )
            {
                //create a level record, then get next
                LWB.StandardItem item = new LWB.StandardItem();
                string desc = GetNodeChildNodeText( statement, "dcterms:description" );
                string levelType = GetNodeChildNodeText( statement, "asn:statementLabel" );
                string statementNotation = GetNodeChildNodeText( statement, "asn:statementNotation" );

                //check if exists, for rerun??

                item.ParentId = parentKey;

                //some may not have domain/cluster. ie NHES will have Standard and performance indicator
                if ( levelType == "" )
                {
                    if ( parentKey == 0 )
                        levelType = "Domain?";
                    else
                        levelType = "Cluster?";
                }
                item.LevelType = levelType;
                item.NotationCode = statementNotation;
                item.Description = desc.Length > 0 ? desc : statement.InnerText;
                item.StandardUrl = url;
                //TODO - get education levels==> N/a, only for actual standard

                newLevelId = myManager.StandardItem_Create( item, ref statusMessage );
                item.Id = newLevelId;
                //generate id for testing
                //parentId++;
                //item.Id = parentId;

                levelsCategory.Add( item.Description );

                LoggingHelper.DoTrace( 1, string.Format( "===== CreatedLevel: {0}: {1}, {2}", parentId, url, item.Description ), false );
                //if parentKey == 0, then should have a top level node (domain), need to connect
                if ( parentKey == 0 )
                {
                    LWB.Standard_SubjectStandardConnector entity = new LWB.Standard_SubjectStandardConnector();
                    entity.StandardSubjectId = currentSubjectId;
                    entity.DomainNodeId = newLevelId;

                    if ( myManager.SubjectStandardConnector_Create( entity, ref statusMessage ) )
                    {
                        LoggingHelper.DoTrace( 1, string.Format( "***** SubjectStandardConnector_Create Successful. subjectId:{0}, domainId:{1}, ", parentKey, newLevelId ) );
                    }
                    else
                    {
                        //failed
                        LoggingHelper.DoTrace( 1, string.Format( "####### SubjectStandardConnector_Create failed. subjectId:{0}, domainId:{1}, ", parentKey, newLevelId ) );

                    }

                }

                foreach ( XmlNode child in children )
                {
                    string topic = GetAttribute( child, "rdf:resource" );
                    if ( topic.Length > 0 )
                    {
                        XmlNode next = GetStatement( doc, topic );
                        //LoggingHelper.DoTrace( 1, "HandleLevel - Next: " + topic );
                        HandleLevel( doc, next, topic, item.Id, ref levelsCategory );
                    }

                } //foreach record
            }
            else
            {
                //should have a standard
                standardId++;

                //LoggingHelper.DoTrace( 1, string.Format( "***** CreatedStandard. id:{0}, parentId:{1}, url:{2}, text:{3}", standardId, parentKey, url, statement.InnerText ) );

                CreateStandard( statement, standardId, parentKey, levelsCategory );
            }

            return newLevelId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="standardId">ONLY USED DURING TESTING, WHEN NOT PERSISTING</param>
        /// <param name="parentKey"></param>
        /// <param name="levelsCategory"></param>
        public void CreateStandard( XmlNode node, int standardId, int parentKey, ArrayList levelsCategory )
        {
            string statusMessage = "";
            LWB.StandardItem item = new LWB.StandardItem();
            item.ParentId = parentKey;
            item.StandardUrl = GetAttribute( node, "rdf:about" );
            //if ( levelsCategory.Count > 0 )
            //    item.MainChName = levelsCategory[ 0 ].ToString();
            //if ( levelsCategory.Count > 1 )
            //    item.Sub1ChName = levelsCategory[ 1 ].ToString();
            //if ( levelsCategory.Count > 2 )
            //{
            //    //issue
            //    item.Sub2ChName = levelsCategory[ 2 ].ToString();
            //    LoggingHelper.DoTrace( 1, string.Format( "********************** Greater Than 2 levels: {0}", item.Sub2ChName ), false );
            //}
            //List<LWB.DataItem> edItems = new List<LWB.DataItem>();
           // ArrayList  edItems = new ArrayList();

            foreach ( XmlNode subchild in node )
            {
                if ( subchild.Name == "dcterms:educationLevel" )
                {
                    string level = GetAttribute( subchild, "rdf:resource" );
                    item.EducationLevels.Add( level );

                }
                else if ( subchild.Name == "asn:statementLabel" )
                {
                    item.LevelType = subchild.InnerText;
                    item.Title = subchild.InnerText;

                }
                else if ( subchild.Name == "dcterms:description" )
                {
                    item.Description = subchild.InnerText;

                }
                else if ( subchild.Name == "skos:exactMatch" )
                {
                    string res = GetAttribute( subchild, "rdf:resource" );
                    if ( res.StartsWith( "http" ) )
                        item.StandardGuid = res;
                    else if ( res.StartsWith( "urn:guid:" ) )
                        item.StandardGuid = res.Substring( 9 );

                }
                else if ( subchild.Name == "asn:statementNotation" )
                {
                    item.NotationCode = subchild.InnerText;

                }
                else if ( subchild.Name == "dcterms:subject" )
                {
                    item.Subject = GetAttribute( subchild, "rdf:resource" );

                }
                else if ( subchild.Name == "dcterms:language" )
                {
                    item.Language = GetAttribute( subchild, "rdf:resource" );

                }
                else if ( subchild.Name == "asn:identifier" )
                {
                    item.AltUrl = GetAttribute( subchild, "rdf:resource" );

                }
               
            }//foreach  child

            //create
            if ( ( item.StandardUrl.Length > 0 || item.NotationCode.Length > 0 )
                && item.Description.Length > 0) 
            {

                standardId = myManager.StandardItem_Create( item, ref statusMessage );
                item.Id = standardId;

                //do other edits
                LoggingHelper.DoTrace( 1, string.Format( "***** CreatedStandard. ", standardId, item.ParentId), false );
                if ( levelsCategory.Count > 0 )
                {
                    int cntr = 0;
                    foreach ( string level in levelsCategory )
                    {
                        cntr++;
                        LoggingHelper.DoTrace( 1, string.Format( "___Level {0}, {1}", cntr, level ), false );
                    }
                }
                LoggingHelper.DoTrace( 1, string.Format( "____id: {0}, \rparentId: {1}, \rsubject: {2} \rcode: {3}, \rLevel: {4}, \rtext:{5}",
                    standardId, item.ParentId, item.Subject, item.NotationCode, item.LevelType, item.Description ), false );


                //create edlevels
                if ( item.EducationLevels.Count > 0 ) 
                {
                    if ( item.EducationLevels.Count > 1 )
                        LoggingHelper.DoTrace( 1, string.Format( "&&&&&&&&& multiple EdLevels: {0}", item.EducationLevels.Count ), false );

                    CreateGradeLevels( standardId, item.EducationLevels);

                }
            }
        }


        private void CreateGradeLevels( int standardId, ArrayList levels)
        {
            string statusMessage = "";
            //
            foreach ( string gl in levels )
            {
                LoggingHelper.DoTrace( 1, string.Format( "___@@@ EdLevels: {0}", gl ), false );
                //LWB.ResourceMap entity = CodeTableManager.GradeLevelGetByUrl( gl );
                LWB.CodeGradeLevel entity = CodeTableManager.GradeLevelGetByUrl( gl );
                if ( entity.IsValid )
                {
                    int id = myManager.StandardGradeLevel_Create( standardId, entity.Id, ref statusMessage );

                }

            }
        }


        private XmlNode GetStatement( XmlDocument doc, string topic )
        {

            XmlNodeList statements = doc.GetElementsByTagName( "asn:Statement" );
            foreach ( XmlNode statement in statements )
            {
                string next = GetAttribute( statement, "rdf:about" );

                if ( next == topic )
                {
                    //found, 
                    //create level one, provide key
                    if ( debugLevel > 5)
                        LoggingHelper.DoTrace( 1, "GetStatement - Found: " + topic, false );

                    return statement;
                }

            } //foreach record

            return null;
        }

        /// <summary>
        /// Retrieve innerText from an XmlNode's child node using specified subChildName
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public string GetNodeChildNodeText( XmlNode node, string subChildName )
        {
            foreach ( XmlNode subchild in node )
            {
                if ( subchild.Name == subChildName )
                {
                    return subchild.InnerText;
                }
            }//foreach  child

            return "";
        }

        public string GetAttribute( XmlNode node, string attributeName )
        {
            string value = "";

            if ( node.Attributes != null )
            {
                XmlAttributeCollection attrsCol = node.Attributes;
                foreach ( XmlAttribute attr in attrsCol )
                {
                    if ( attr.Name == attributeName )
                    {
                        value = attr.Value;
                        break;
                    }
                }
            }
            return value;
        }


        protected void Extract( string response, string responseName )
        {
            Address address = new Address();
            string result = "";

            try
            {
                XmlDocument xmlReply = new XmlDocument();
                xmlReply.LoadXml( response );

                foreach ( XmlNode child in xmlReply )
                {
                    if ( child.Name == responseName )
                    {
                        foreach ( XmlNode addrChild in child )
                        {
                            if ( addrChild.Name == "Address" )
                            {
                                foreach ( XmlNode subchild in addrChild )
                                {
                                    if ( subchild.Name == "FirmName" )
                                    {
                                        //txtAddress1.Text = subchild.InnerText + "<br/>";
                                        address.TempProperty1 = subchild.InnerText;

                                    }
                                    else if ( subchild.Name == "Address1" )
                                    {
                                        //txtAddress1.Text += subchild.InnerText;
                                        address.Address1 = subchild.InnerText;

                                    }
                                    else if ( subchild.Name == "Address2" )
                                    {
                                        //txtAddress2.Text = subchild.InnerText;
                                        address.Address2 = subchild.InnerText;

                                    }
                                    else if ( subchild.Name == "City" )
                                    {
                                        //txtCity.Text = subchild.InnerText;
                                        address.City = subchild.InnerText;

                                    }
                                    else if ( subchild.Name == "State" )
                                    {
                                        //txtState.Text = subchild.InnerText;
                                        address.State = subchild.InnerText;

                                    }
                                    else if ( subchild.Name == "Zip5" )
                                    {
                                        //txtZip.Text = subchild.InnerText;
                                        address.ZipCode = subchild.InnerText;

                                    }
                                    else if ( subchild.Name == "Zip4" )
                                    {
                                        //txtZip4.Text = subchild.InnerText;
                                        address.ZipCodePlus4 = subchild.InnerText;

                                    }
                                }//foreach address child
                            }
                        }
                        break;
                    }
                }
            }
            catch ( Exception ex )
            {
                result = "ExtractNode error: " + ex.Message;
            }

            result += address.HtmlFormat();
        }

    }
}