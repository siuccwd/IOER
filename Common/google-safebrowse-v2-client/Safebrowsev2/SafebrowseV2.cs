/*****************************************************************************
 * Licensed to Qualys, Inc. (QUALYS) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * QUALYS licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *****************************************************************************/

// Documentation for V2 Google Safe Browsing APIs are here:
// http://code.google.com/apis/safebrowsing/developers_guide_v2.html

// Author: Brian Cheek (Qualys) 8/2010
// Maintainer & Support Contact: Patrick Thomas (Qualys), pthomas@qualys.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Diagnostics;

// Assumptions: It is assumed the MasterList[0] = malware blacklist, MasterList[1] = malware whitelist,
//                                MasterList[2] = phish blacklist, MasterList[3] = phish whitelist

namespace SafebrowseV2
{
    public class ReputationEventing
    {
        // This call registers the event source.
        // This call requires elevation, otherwise it will throw an exception.
        // It only needs to be done once per machine.
        // It should be done before running the reputation service (but not immediately before).
        public void InitializeEventing()
        {
            // Create the event source, if it does not already exist.
            if (false == EventLog.SourceExists("ReputationService"))
            {
                EventLog.CreateEventSource("ReputationService", "Application");
            }
        }

        // This call unregisters the event source.
        // This call requires elevation, otherwise it will throw an exception.
        // It only needs to be done once per machine.
        // It should be done after quitting the reputation service (but not immediately after).
        public void ShutdownEventing()
        {
            if (true == EventLog.SourceExists("ReputationService"))
            {
                EventLog.DeleteEventSource("ReputationService");
            }
        }
    }

    [Flags]
    public enum Reputation
    {
        None = 0x0,
        MalwareBlackList = 0x1 << 0,
        PhishBlackList = 0x1 << 1,
        Error = 0x1 << 2
        // This is stored as an 8 bit value, the maximum value is 0x1 << 7
    }

    public class ReputationEngine
    {
        private SafeBrowsing safeBrowsing;
        public SafeBrowsing SafeBrowser
        {
            get { return this.safeBrowsing; }
            set { this.safeBrowsing = value; }
        }
        private static EventLog eventLog = null;
        private string DefaultCacheDir = @"C:\ReputationService\";
        

        public void Initialize(string ApiKey)
        {
            Initialize(ApiKey, DefaultCacheDir);
        }

        public void Initialize(string apiKey, string cacheBaseDir)
        {
            try
            {
                if ((eventLog == null) && EventLog.SourceExists("ReputationService"))
                {
                    eventLog = new EventLog("Application", ".", "ReputationService");
                }
            }
            catch (System.Security.SecurityException sx)
            {
                // Do nothing - we just won't log any errors
                // TODO: Alternate error logging
            }

            if ((string.IsNullOrEmpty(apiKey)) ||
                (string.IsNullOrEmpty(cacheBaseDir)))
            {
                if (eventLog != null) eventLog.WriteEntry("null or string.Empty values passed into Initialize().", EventLogEntryType.Warning);
                return;
            }

            try
            {
                safeBrowsing = new SafeBrowsing(apiKey, cacheBaseDir);
            }
            catch
            {
                if (eventLog != null) eventLog.WriteEntry("No local cache for google lists.", EventLogEntryType.Warning);
            }
        }

        public Reputation CheckUrl(string Url)
        {
            return safeBrowsing.CheckUrl(Url);
        }

        public List<Reputation> CheckUrl(List<string> Urls)
        {
            return safeBrowsing.CheckUrl(Urls);
        }
        
        // Each master list is a dictionary of "chunk#'s / chunks" (this is the outer dictionary)
        public class MasterList // was protected not public
        {
            static public MasterList[] CreateAndInitArray(uint count)
            {
                MasterList[] array = new MasterList[count];
                for (int index = 0; count > index; index++)
                {
                    array[index] = new MasterList();
                }

                return array;
            }

            static public ReaderWriterLockSlim listLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

            public bool fUpToDate = false;
            public Dictionary<uint, Chunk> e = new Dictionary<uint, Chunk>();

            // Disk format:
            // bool, uint, uint, dictionary.count(int)
            // For each dictionary entry: uint, Chunk (see below)

            // Writes the contents of this object to disk
            public void WriteToDisk(BinaryWriter bw)
            {
                bw.Write(fUpToDate);
                bw.Write(e.Count);
                foreach (KeyValuePair<uint, Chunk> kvp in e)
                {
                    bw.Write(kvp.Key);
                    kvp.Value.WriteToDisk(bw);
                }
            }

            // Reads the contents of this object to disk
            public void ReadFromDisk(BinaryReader br)
            {
                e.Clear();
                fUpToDate = br.ReadBoolean();
                int count = br.ReadInt32();
                for (int index = 0; count > index; index++)
                {
                    uint key = br.ReadUInt32();
                    Chunk value = new Chunk();
                    value.ReadFromDisk(br);
                    e.Add(key, value);
                }
            }
        }

        // Each Chunk object is a dictionary of "SHA256 host key hash fragment (little endian uint built from
        // a byte[4]) / dictionary of HostKeyData" (this is the middle dictionary)
        public class Chunk // was protected not public
        {
            public Dictionary<uint, HostKeyData> e = new Dictionary<uint, HostKeyData>();

            // Disk format:
            // dictionary.count(int)
            // For each dictionary entry: uint, HostKeyData (see below)

            // Writes the contents of this object to disk
            public void WriteToDisk(BinaryWriter bw)
            {
                bw.Write(e.Count);
                foreach (KeyValuePair<uint, HostKeyData> kvp in e)
                {
                    bw.Write(kvp.Key);
                    kvp.Value.WriteToDisk(bw);
                }
            }

            // Reads the contents of this object to disk
            public void ReadFromDisk(BinaryReader br)
            {
                e.Clear();
                int count = br.ReadInt32();
                for (int index = 0; count > index; index++)
                {
                    uint key = br.ReadUInt32();
                    HostKeyData value = new HostKeyData();
                    value.ReadFromDisk(br);
                    e.Add(key, value);
                }
            }
        }

        // Each HostKeyData object contains the add chunk reference (for sub chunks), and a dictionary of
        // "SHA256 hash prefixes (byte []) / add chunk references (for sub chunks)" (this is the inner dictionary)
        // Note, if there are zero entries in this dictionary, then the addChunkReference value is in use. If
        // there are any entries in the dictionary, then the value in the dictionary is in use and addChunkReference
        // is not in use.
        public class HostKeyData // was protected not public
        {
            public uint addChunkReference;
            public Dictionary<byte[], uint> e = new Dictionary<byte[], uint>();

            // Disk format:
            // uint, dictionary.count(int)
            // For each dictionary entry: size(int), byte[], uint

            // Writes the contents of this object to disk
            public void WriteToDisk(BinaryWriter bw)
            {
                bw.Write(addChunkReference);
                bw.Write(e.Count);
                foreach (KeyValuePair<byte[], uint> kvp in e)
                {
                    bw.Write(kvp.Key.Length);
                    bw.Write(kvp.Key);
                    bw.Write(kvp.Value);
                }
            }

            // Reads the contents of this object to disk
            public void ReadFromDisk(BinaryReader br)
            {
                e.Clear();
                addChunkReference = br.ReadUInt32();
                int count = br.ReadInt32();
                for (int index = 0; count > index; index++)
                {
                    int length = br.ReadInt32();
                    byte[] key = br.ReadBytes(length);
                    uint value = br.ReadUInt32();
                    e.Add(key, value);
                }
            }
        }

        public class SafeBrowsing // (was protected not public)
        {
            private const int DefaultWebRetryCount = 1;
            private const int DefaultWebRetryWaitTimeInMilliseconds = 1000;
            private const int SafeBrowsingRequestTimeoutInMilliseconds = 2000;

            private const string CommandList = "list";
            private const string CommandGetHash = "gethash";
            private const string CommandDownloads = "downloads";
            private const int ListQuantity = 2;
            private const string ListMalware = "goog-malware-shavar";
            private const string ListPhish = "googpub-phish-shavar";
            private const string ListRegTest = "goog-regtest-shavar";
            private const string ListWhiteDomain = "goog-whitedomain-shavar";
            private const string BaseUrl = @"http://safebrowsing.clients.google.com/safebrowsing/";
            private const string ClientApiAndApiKeyTag = @"?client=api&apikey=";
            private const string AppVerAndPVer = @"&appver=2.0.0&pver=2.2";
            private const string CacheBaseName = @"SafeBrowsingCacheList";

            private static string ApiKey = null;
            private static string CacheBaseDir = null;

            // The quickLookupCache is a dictionary that contains hash prefixes and which lists those hash prefixes exist
            // The key is a 32 bit hash prefix, stored in a UInt32 in little endian format. The value is the Reputation enum
            // This dictionary is built at initialization time after reading the cache from disk, and also built after an update
            // from google's servers.
            // Note: This dictionary only works with 32 bit hash prefixes - this will need to be changed if google starts using
            // hash prefixes of other lengths. This dictionay does not support whitelist lookups.
            static volatile public Dictionary<UInt32, Reputation> quickLookupCache = new Dictionary<UInt32, Reputation>();

            // The fullHashCache is a list that contains full hashes retrieved from google.
            // Anytime there will be an attempted hash lookup, this list will be checked first to prevent repeated requests to
            // the google server.
            // Anytime a full hash is returned from google, it is added to this list.
            // This list is erased every time the quickLookupCache is rebuilt.
            public class HashAndReputation
            {
                public HashAndReputation(byte[] newHash, Reputation newReputation) { hash = newHash; reputation = newReputation; }
                public byte[] hash;
                public Reputation reputation;
            }
            static volatile public List<HashAndReputation> fullHashCache = new List<HashAndReputation>();

            MasterList[] masterLists = MasterList.CreateAndInitArray(ListQuantity * 2);
            public MasterList[] MasterLists
            {
                get { return this.masterLists; }
                set { this.masterLists = value; }
            }

            Thread listRetrieverThread;

            static private void PopulateQuickLookupCache(ref MasterList[] reputationLists)
            {
                Dictionary<UInt32, Reputation> newQuickLookupCache = new Dictionary<UInt32, Reputation>();

                for (int index = 0; reputationLists.Length > index; index += 2)
                {
                    foreach (Chunk chunk in reputationLists[index].e.Values)
                    {
                        foreach (uint key in chunk.e.Keys)
                        {
                            if (newQuickLookupCache.ContainsKey(key))
                            {
                                newQuickLookupCache[key] |= ((index == 0) ? Reputation.MalwareBlackList : Reputation.PhishBlackList);
                            }
                            else
                            {
                                newQuickLookupCache.Add(key, ((index == 0) ? Reputation.MalwareBlackList : Reputation.PhishBlackList));
                            }
                        }
                    }
                }

                // Hotswapping the volatile pointer doesn't require a lock.
                quickLookupCache = newQuickLookupCache;
            }

            public SafeBrowsing(string apiKey, string cacheBaseDir)
            {
                ApiKey = apiKey;
                CacheBaseDir = cacheBaseDir;

                // Read master lists from disk
                MasterList.listLock.EnterWriteLock();
                try
                {
                    for (int indexReader = 0; (ListQuantity * 2) > indexReader; indexReader++)
                    {
                        try
                        {
                            BinaryReader br = new BinaryReader(File.Open(CacheBaseDir + @"\" + CacheBaseName + indexReader.ToString() + @".bin", FileMode.Open));
                            masterLists[indexReader].ReadFromDisk(br);
                            br.Close();
                        }
                        catch(Exception e) 
                        {
                            if (eventLog != null) eventLog.WriteEntry("Failed to read Master List from disk: " + e.Message, EventLogEntryType.Warning);
                        }
                    }

                    PopulateQuickLookupCache(ref masterLists);
                    List<HashAndReputation> newFullHashCache = new List<HashAndReputation>();
                    // Hotswapping the volatile pointer doesn't require a lock.
                    fullHashCache = newFullHashCache;
                }
                finally
                {
                    MasterList.listLock.ExitWriteLock();
                }

            }

            public bool Shutdown()
            {
                // Not implemented
                return true;
            }

            // Call SafeBrowsing API to get new/updated list data
            static public bool UpdateList(MasterList[] masterLists)
            {
                // 1. Get the supported lists from the Safe Browsing API
                string lists = GetStringFromUrl(MakeSafebrowseCommandUrl(CommandList), String.Empty);

                // Confirm the desired lists exist in the response
                if ((string.Empty == lists) ||
                    (null == lists) ||
                    (0 > lists.IndexOf(ListMalware)) ||
                    (0 > lists.IndexOf(ListPhish))) //||
                    //(0 > lists.IndexOf(ListRegTest)) ||
                    //(0 > lists.IndexOf(ListWhiteDomain)))
                {
                    return false;
                }

                // 2. Get the list of redirect URLs
                for (int index = 0; index < ListQuantity; index++)
                {
                    MasterList.listLock.EnterWriteLock();
                    try
                    {
                        // Reset the dirty flags on the master lists
                        masterLists[index * 2].fUpToDate = false;
                        masterLists[(index * 2) + 1].fUpToDate = false;

                        // Make continued requests until no data is returned by the server, which signifies that we are up to date
                        while ((false == masterLists[index * 2].fUpToDate) || (false == masterLists[(index * 2) + 1].fUpToDate))
                        {
                            string redirectUrls;
                            // Set the index of the low and high chunk numbers that are cached, which is used when requesting more data
                            // I.e. Tell the Safe Browsing API what we have and it returns what we don't have

                            List<uint> list = new List<uint>();
                            foreach (KeyValuePair<uint, Chunk> kvp in masterLists[index * 2].e)
                            {
                                list.Add(kvp.Key);
                            }
                            string aRange = CreateRangeStringFromUintList(list);
                            list.Clear();
                            foreach (KeyValuePair<uint, Chunk> kvp in masterLists[(index * 2) + 1].e)
                            {
                                list.Add(kvp.Key);
                            }
                            string sRange = CreateRangeStringFromUintList(list);

                            switch (index)
                            {
                                case 0:
                                    redirectUrls = GetRedirectUrls(ListMalware, aRange, sRange);
                                    break;
                                case 1:
                                    redirectUrls = GetRedirectUrls(ListPhish, aRange, sRange);
                                    break;
                                default:
                                    // Assert - should never occur
                                    return false;
                                //break;
                            }

                            // DEBUG BEGIN
                            // Write list of redirect URLs to disk
                            //StreamWriter writer = new StreamWriter(@"c:\dsmbd\RedirectUrls" + index.ToString() + @".txt");
                            //writer.WriteLine(redirectUrls);
                            //writer.Close();
                            // DEBUG END

                            // Retrieve the list of add chunks to delete (for example ad:1500-1732)
                            List<uint> addChunksToDelete = RetrieveDelChunkList(redirectUrls, true);
                            // Remove each chunk from the master list
                            foreach (uint addChunk in addChunksToDelete)
                            {
                                masterLists[index * 2].e.Remove(addChunk);
                            }

                            // Retrieve the list of sub chunks to delete (for example sd:1500-1732)
                            List<uint> subChunksToDelete = RetrieveDelChunkList(redirectUrls, false);
                            // Remove each chunk from the master list
                            foreach (uint subChunk in subChunksToDelete)
                            {
                                masterLists[(index * 2) + 1].e.Remove(subChunk);
                            }

                            byte[] rawChunkData = RetrieveAndMergeRedirectData(redirectUrls);

                            // DEBUG BEGIN
                            // Write the binary blob to disk
                            //BinaryWriter bw = new BinaryWriter(File.Open(@"c:\dsmbd\Raw" + index.ToString() + @".txt", FileMode.Create));
                            //bw.Write(rawChunkData);
                            //bw.Close();
                            // DEBUG END

                            // Merge data with existing lists
                            if (0 != rawChunkData.Length)
                            {
                                MergeRawDataIntoMasterList(rawChunkData, ref masterLists, index * 2);
                            }
                            else
                            {
                                // If there is no new data, then the master list is up to date
                                masterLists[index * 2].fUpToDate = true;
                                masterLists[(index * 2) + 1].fUpToDate = true;
                            }
                        }

                        for (int indexWriter = 0; 2 > indexWriter; indexWriter++)
                        {
                            // Write master list to disk
                            try
                            {
                                BinaryWriter bw = new BinaryWriter(File.Open(CacheBaseDir + @"\" + CacheBaseName + ((index * 2) + indexWriter).ToString() + @".bin", FileMode.Create));
                                masterLists[(index * 2) + indexWriter].WriteToDisk(bw);
                                bw.Close();
                            }
                            catch(Exception e)
                            {
                                if (eventLog != null) eventLog.WriteEntry("Failed to persist Master Lists to disk: " + e.Message, EventLogEntryType.Warning);
                            }
                        }

                        PopulateQuickLookupCache(ref masterLists);
                        List<HashAndReputation> newFullHashCache = new List<HashAndReputation>();
                        // Hotswapping the volatile pointer doesn't require a lock.
                        fullHashCache = newFullHashCache;
                    }
                    finally
                    {
                        MasterList.listLock.ExitWriteLock();
                    }
                }

                return true;
            }

            private static string MakeSafebrowseCommandUrl(string command)
            {
                return BaseUrl + command + ClientApiAndApiKeyTag + ApiKey + AppVerAndPVer;
            }

            // Give it an URL and an optional requestBody, get a response from the server as a string
            static protected string GetStringFromUrl(string URL, string requestBody)
            {
                WebRequest request = WebRequest.Create(URL);
                if (false == String.IsNullOrEmpty(requestBody))
                {
                    request.Method = "POST";
                    byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteArray.Length;

                    // Get the request stream.
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(byteArray, 0, byteArray.Length);
                    requestStream.Close();
                }

                int retryCount = 0;
            Retry:
                WebResponse response = null;
                try
                {
                    response = request.GetResponse();
                    // Tip: The status string is: ((HttpWebResponse)response).StatusDescription;
                }
                catch (WebException e)
                {
                    if (retryCount < DefaultWebRetryCount)
                    {
                        retryCount++;
                        Thread.Sleep(DefaultWebRetryWaitTimeInMilliseconds);
                        goto Retry;
                    }
                    else
                    {
                        throw new Exception(e.Message, e);
                    }
                }

                // Get the stream containing content returned by the server
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string responseFromServer = reader.ReadToEnd();

                // Clean up the streams
                reader.Close();
                responseStream.Close();
                response.Close();

                return responseFromServer;
            }

            // Give it an URL and an optional requestBody, get a response from the server as a byte array
            static protected byte[] GetByteArrayFromUrl(string URL, byte[] requestBody)
            {
                WebRequest request = WebRequest.Create(URL);
                if (null != requestBody)
                {
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = requestBody.Length;

                    // Get the request stream.
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(requestBody, 0, requestBody.Length);
                    requestStream.Close();
                }

                int retryCount = 0;
            Retry:
                WebResponse response = null;
                try
                {
                    request.Timeout = SafeBrowsingRequestTimeoutInMilliseconds;
                    response = request.GetResponse();
                    // Tip: The status string is: ((HttpWebResponse)response).StatusDescription;
                }
                catch (WebException e)
                {
                    if (retryCount < DefaultWebRetryCount)
                    {
                        retryCount++;
                        Thread.Sleep(DefaultWebRetryWaitTimeInMilliseconds);
                        goto Retry;
                    }
                    else
                    {
                        throw new Exception("GetByteArrayFromUrl():\n\n" + e.Message, e);
                    }
                }

                // Get the stream containing content returned by the server
                Stream responseStream = response.GetResponseStream();

                // Copy responseStream to memoryStream so we can determine size and create a byte array from it
                MemoryStream memoryStream = new MemoryStream();
                byte[] memoryStreamBuffer = new byte[1024];
                int totalBytesRead = 0;
                int bytesRead = 0;

                while (0 < (bytesRead = responseStream.Read(memoryStreamBuffer, 0, memoryStreamBuffer.Length)))
                {
                    totalBytesRead += bytesRead;
                    memoryStream.Write(memoryStreamBuffer, 0, bytesRead);
                }

                memoryStream.Position = 0;

                // Copy memoryStream to byte array
                byte[] responseFromServer = new byte[totalBytesRead];
                memoryStream.Read(responseFromServer, 0, totalBytesRead);

                // Clean up the streams
                memoryStream.Close();
                responseStream.Close();
                response.Close();

                return responseFromServer;
            }

            // Retrieve the list of redirect URLs from the server
            // The aRange is the a: chunks, the sRange is the s: chunks
            static private string GetRedirectUrls(string list, string aRange, string sRange)
            {
                // Create the "a:1234-5678" string used in the request body
                string aChunks = (String.Empty != aRange) ? "a:" + aRange : String.Empty;

                // Create the "s:1234-5678" string used in the request body
                string sChunks = (String.Empty != sRange) ? "s:" + sRange : String.Empty;

                string separator = "";
                if ((false == string.IsNullOrEmpty(aChunks)) && (false == string.IsNullOrEmpty(sChunks)))
                {
                    separator = ":";
                }

                return GetStringFromUrl(MakeSafebrowseCommandUrl(CommandDownloads),
                    list + ";" + aChunks + separator + sChunks + "\n");
            }

            // This returns a new byte array that concatenates the two byte arrays passed in
            static private byte[] ConcatenateByteArray(byte[] array1, byte[] array2)
            {
                byte[] newArray = new byte[array1.Length + array2.Length];

                Buffer.BlockCopy(array1, 0, newArray, 0, array1.Length);
                Buffer.BlockCopy(array2, 0, newArray, array1.Length, array2.Length);

                return newArray;
            }

            // Create a range string from a uint list. For example, if the list
            // is "1, 2, 3, 4, 6, 7" then the string is "1-4,6-7"
            // This will properly handle out of order lists and duplicate entries
            static private string CreateRangeStringFromUintList(List<uint> list)
            {
                string result = String.Empty;

                // Sort list in ascending order
                list.Sort();
                // Remove dupes
                list = list.Distinct().ToList();
                if (list.Count == 0)
                {
                    // Empty list was passed in
                    return String.Empty;
                }

                // Logic: Start a 'run' of values. With every new value, see if it continues the run (1 more than the last entry).
                // If not, write the run to the string (including if the run is 1 number wide)
                // The last iteration of the loop completes the string
                uint lowValue = list[0];
                uint highValue = list[0];
                uint previous = list[0];
                for (int index = 1; index <= list.Count; index++)
                {
                    // If it's not the last iteration and the current entry is 1 more than the last one
                    if ((index != list.Count) && (list[index] == previous + 1))
                    {
                        // Then update the previous entry and continue searching
                        previous = list[index];
                    }
                    else
                    {
                        // Write the lowValue to highValue run to the string
                        highValue = previous;
                        if (result != String.Empty)
                        {
                            // Add a comma if this isn't the first run
                            result += ",";
                        }
                        // Write the low number of the run
                        result += lowValue.ToString();
                        if (lowValue != highValue)
                        {
                            // If the run isn't just a single number, write the dash and the high number of the run
                            result += "-" + highValue.ToString();
                        }
                        // Only update the values if it isn't the last iteration (which is 1 past the end of the list count)
                        if (index != list.Count)
                        {
                            lowValue = list[index];
                            previous = list[index];
                        }
                    }
                }

                return result;
            }

            static private bool IsNumber(char value)
            {
                if (('0' <= value) && ('9' >= value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Pass in a string, start index, and you will get a number built from that string
            // up until the separator character (a dash, comma, or end of string),
            // as well as the separator type itself, and a pointer to the separator character
            static private void GetNumberAndSeparatorType(int indexNumberStart, string range, out uint number, out SeparatorType separatorType, out int indexSeparator)
            {
                int index = indexNumberStart;
                separatorType = SeparatorType.Uninitialized;
                // Determine the start and end index of the number represented in the string
                while (index < range.Length)
                {
                    // Determine the type of separator character
                    if (range[index] == '-')
                    {
                        separatorType = SeparatorType.Dash;
                        break;
                    }
                    else if (range[index] == ',')
                    {
                        separatorType = SeparatorType.Comma;
                        break;
                    }
                    index++;
                }
                indexSeparator = index;
                // End of string hit - note that as the type of separator
                if (separatorType == SeparatorType.Uninitialized)
                {
                    separatorType = SeparatorType.End;
                }

                // Build the uint from the string
                if (index != range.Length)
                {
                    number = Convert.ToUInt32(range.Substring(indexNumberStart, index - indexNumberStart));
                }
                else
                {
                    number = Convert.ToUInt32(range.Substring(indexNumberStart));
                }
            }

            enum SeparatorType {Uninitialized, Dash, Comma, End};

            // Pass in a range, such as "123-456,555,641-643" and this will return a list of uints of those numbers
            static private List<uint> CreateUintListFromRangeString(string range)
            {
                List<uint> result = new List<uint>();
                int index = 0;
                if (true == String.IsNullOrEmpty(range))
                {
                    return result;
                }
                // Assert it is a valid range
                // First character must be a number, last character must be a number
                if ((false == IsNumber(range[0])) || (false == IsNumber(range[range.Length - 1])))
                {
                    return result;
                }
                // Characters to the left and right of each '-' must be a number
                // Characters to the left and right of each ',' must be a number
                for (index = 1; index < range.Length - 1; index++)
                {
                    if (('-' == range[index]) || (',' == range[index]))
                    {
                        if ((false == IsNumber(range[index - 1])) || (false == IsNumber(range[index + 1])))
                        {
                            return result;
                        }
                    }
                }
                index = 0;
                uint lowNumber = 0;
                uint highNumber = 0;
                int indexNumberStart = index;
                int indexSeparator = 0;
                SeparatorType separatorType = SeparatorType.Uninitialized;
                // Iterate through the string to find the runs (ranges of low-high) and individual values
                while (index < range.Length)
                {
                    // Get the low number
                    GetNumberAndSeparatorType(index, range, out lowNumber, out separatorType, out indexSeparator);

                    // If there's a dash, then get the high number
                    if (separatorType == SeparatorType.Dash)
                    {
                        GetNumberAndSeparatorType(indexSeparator + 1, range, out highNumber, out separatorType, out indexSeparator);
                    }
                    else
                    {
                        // There was no dash, so the low number = the high number
                        highNumber = lowNumber;
                    }

                    // Swap the numbers if the low number is > the high number
                    if (lowNumber > highNumber)
                    {
                        uint temp = lowNumber;
                        lowNumber = highNumber;
                        highNumber = temp;
                    }

                    // Add these values to the list
                    for (uint count = lowNumber; count <= highNumber; count++)
                    {
                        result.Add(count);
                    }

                    // Continue until the end of the string is hit
                    if (separatorType == SeparatorType.End)
                    {
                        break;
                    }

                    // Move the pointer to the beginning of the next number
                    index = indexSeparator + 1;
                }

                return result;
            }

            // if isAddDel = true, the tag is "ad:". If false, the tag is "sd:"
            static private List<uint> RetrieveDelChunkList(string delChunkList, bool isAddDel)
            {
                List<uint> result = new List<uint>();
                string tag = (true == isAddDel) ? "ad:" : "sd:";

                // Format of delChunkList (string that references the del chunks)
                // Each entry is ad:###\n or sd:###\n
                // ### can be a single number, range (4-7), or any combination (1,4-7,10)
                // EOF determines end of list
                string remainder = delChunkList;

                // Read the string expression after the "ad:" or "sd:"
                int index = 0;
                while ((String.Empty != remainder) && (0 <= (index = remainder.IndexOf(tag))))
                {
                    remainder = remainder.Substring(index + tag.Length);
                    string line = remainder.Substring(0, remainder.IndexOf('\n'));

                    // Get the list of uints from this line
                    List<uint> rangeList = CreateUintListFromRangeString(line);
                    foreach (uint entry in rangeList)
                    {
                        result.Add(entry);
                    }
                }
                return result;
            }

            // This retrieves all the raw chunk data for all the redirect URLs, merges them, and returnes the merged data.
            static private byte[] RetrieveAndMergeRedirectData(string redirectUrls)
            {
                byte[] mergedRawChunkData = new byte[0];

                // Format of redirectUrls (string that references the URLs which point to the actual data)
                // Each entry is u:URL\n
                // EOF determines end of list
                string remainder = redirectUrls;

                // Find the URL after the "u:", and if it exists, get data from that URL
                int index = 0;
                while ((String.Empty != remainder) && (0 <= (index = remainder.IndexOf("u:"))))
                {
                    remainder = remainder.Substring(index + 2);
                    byte[] rawChunkData = GetByteArrayFromUrl("http://" + remainder.Substring(0, remainder.IndexOf('\n')), null);
                    mergedRawChunkData = ConcatenateByteArray(mergedRawChunkData, rawChunkData);
                }

                return mergedRawChunkData;
            }

            // Get a Uint from a set of ASCII characters in a byte array up until the terminating character
            // Increment index to point to the byte after the terminating character
            static private bool GetUintFromString(byte[] array, ref uint index, byte terminatingCharacter, out uint number)
            {
                bool fSuccess = false;

                number = 0;
                while (terminatingCharacter != array[index])
                {
                    if ((array[index] >= (byte)'0') && (array[index] <= (byte)'9'))
                    {
                        fSuccess = true;
                        number *= 10;
                        number += (uint)array[index] - (byte)'0';
                    }
                    else
                    {
                        // A non-number and non-terminating character was found
                        fSuccess = false;
                        break;
                    }

                    index++;
                }

                // Point to the character after the terminating character
                if (true == fSuccess)
                {
                    index++;
                }

                return fSuccess;
            }

            // Get a Uint stored in big endian format from a byte array, starting at offset index
            static private uint GetBigEndianUintFromByteArray(ref byte[] array, ref uint index)
            {
                uint number = 0;
                uint endIndex = index + 4;
                for (; endIndex > index; index++)
                {
                    number *= 256;
                    number += array[index];
                }

                return number;
            }

            // Get a Uint stored in little endian format from a byte array
            static private uint GetLittleEndianUintFromByteArray(ref byte[] array)
            {
                uint number = 0;
                for (int index = 3; 0 <= index; index--)
                {
                    number *= 256;
                    number += array[index];
                }

                return number;
            }

            // Take the concatenated set of redirect URL data, parse them, and populate the master list
            static private bool MergeRawDataIntoMasterList(byte[] rawChunkData, ref MasterList[] masterLists, int masterListsOffset)
            {
                uint index = 0;

                while (rawChunkData.Length > index)
                {
                    bool fIsAddChunk = false;
                    // Read chunk type (add or sub)
                    if ((('a' != rawChunkData[index]) && ('s' != rawChunkData[index])) ||
                        (':' != rawChunkData[index + 1]))
                    {
                        // Invalid file format
                        return false;
                    }

                    if ('a' == rawChunkData[index])
                    {
                        fIsAddChunk = true;
                    }

                    // Move the position marker past the "?:"
                    index += 2;

                    // Read chunk number
                    uint chunkNumber = 0;
                    if (false == GetUintFromString(rawChunkData, ref index, (byte)':', out chunkNumber))
                    {
                        // Invalid file format
                        return false;
                    }

                    // Pick the appropriate master list to put this data into (based on whether it's an add or sub chunk)
                    int masterListsIndex = masterListsOffset + ((true == fIsAddChunk) ? 0 : 1);

                    // Read size of hash prefix (not size of hash key)
                    uint cbHashPrefix = 0;
                    if (false == GetUintFromString(rawChunkData, ref index, (byte)':', out cbHashPrefix))
                    {
                        // Invalid file format
                        return false;
                    }

                    // Read size of chunk
                    uint cbChunk = 0;
                    if (false == GetUintFromString(rawChunkData, ref index, 0x0A, out cbChunk))
                    {
                        // Invalid file format
                        return false;
                    }

                    Chunk chunk = new Chunk();

                    // Iterate through chunk binary blob and read all hash keys
                    // Add hash keys to dictionary
                    uint endIndex = index + cbChunk;
                    for (; endIndex > index; )
                    {
                        // Read the host key
                        byte[] hostKey = new byte[4];
                        Buffer.BlockCopy(rawChunkData, (int)index, hostKey, 0, hostKey.Length);
                        index += (uint)hostKey.Length;

                        HostKeyData hostKeyData = new HostKeyData();
                        uint prefixCount = rawChunkData[index++];

                        // If it's a sub chunk and there are no prefixes, then get the add chunk reference
                        if ((false == fIsAddChunk) && (0 == prefixCount))
                        {
                            hostKeyData.addChunkReference = GetBigEndianUintFromByteArray(ref rawChunkData, ref index);
                        }
                        else
                        {
                            hostKeyData.addChunkReference = 0;
                            // Iterate through the count of hash key prefixes (and add chunk references, if this is a sub chunk)
                            for (int prefixIndex = 0; prefixCount > prefixIndex; prefixIndex++)
                            {
                                uint hashPrefixAddChunkReference = 0;
                                if (false == fIsAddChunk)
                                {
                                    hashPrefixAddChunkReference = GetBigEndianUintFromByteArray(ref rawChunkData, ref index);
                                }
                                // Read the hash prefix
                                byte[] hashPrefix = new byte[cbHashPrefix];
                                Buffer.BlockCopy(rawChunkData, (int)index, hashPrefix, 0, hashPrefix.Length);
                                index += (uint)hashPrefix.Length;

                                // Add/replace hash prefix entry to HostKeyData object
                                if (hostKeyData.e.ContainsKey(hashPrefix))
                                {
                                    hostKeyData.e.Remove(hashPrefix);
                                }
                                hostKeyData.e.Add(hashPrefix, hashPrefixAddChunkReference);
                            }
                        }

                        uint hostKeyNumber = GetLittleEndianUintFromByteArray(ref hostKey);

                        // Add/replace chunk entry to chunk object
                        if (chunk.e.ContainsKey(hostKeyNumber))
                        {
                            chunk.e.Remove(hostKeyNumber);
                        }
                        chunk.e.Add(hostKeyNumber, hostKeyData);
                    }

                    // Add/replace chunk dictionary to master chunk list
                    if (masterLists[masterListsIndex].e.ContainsKey(chunkNumber))
                    {
                        masterLists[masterListsIndex].e.Remove(chunkNumber);
                    }
                    masterLists[masterListsIndex].e.Add(chunkNumber, chunk);
                }

                return true;
            }

            // Check a single URL against the master list of hash prefixes
            public Reputation CheckUrl(string Url)
            {
                List<string> Urls = new List<string> { Url };
                List<Reputation> results = CheckUrl(Urls);
                return results[0];
            }

            // Check a list of URLs against the master list of hash prefixes
            // The return is a list of reputations that correspond 1:1 to the respective Urls in the passed-in list
            public List<Reputation> CheckUrl(List<string> Urls)
            {
                const int cbSha256 = 32;

                // reputationResults eventually contains a list of current officially bad Urls
                // (after a round-trip check to the Google server)
                List<Reputation> reputationResults = new List<Reputation>();
                // hashedUrlList contains a list of list of hashed Urls that correspond 1:1
                // to the respective Urls in the passed in list.
                // In other words, each Url will always have an associated list of list of
                // byte arrays, but the inner list of byte arrays might be empty.
                List<List<byte[]>> hashedUrlList = new List<List<byte[]>>();

                // This contains the list of all hashes that will be sent to the Google server
                List<byte[]> hashList = new List<byte[]>();

                for (int index = 0; index < Urls.Count; index++)
                {
                    // First parse the Url through TryCreate. It is possible that this will reject checking
                    // some URLs that might be accepted by different browsers.
                    string url = Urls[index];
                    Uri uri;
                    if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                    {
                        reputationResults.Add(Reputation.None);
                        hashedUrlList.Add(new List<byte[]>());
                    }
                    else
                    {
                        reputationResults.Add(Reputation.Error);
                        hashedUrlList.Add(new List<byte[]>());
                        continue;
                    }

                    // First find out which lists we think it might be in. Note: We're only comparing the first
                    // 4 bytes of the hash, so this is a possible match but not a guaranteed match. We must make a round
                    // trip to the server to retrieve the full hash and determine an actual match

                    List<string> cleanedUrls = Canonicalize(Urls[index]);

                    // For each canonicalized entry in the list, make a SHA256 hash, compare that hash to
                    // our cached hash prefixes, and make a list of the ones that match
                    foreach (string cleanedUrl in cleanedUrls)
                    {
                        byte[] hashedUrl = MakeSHA256Hash(cleanedUrl);

                        // Check the hashedUrl against the fullHashCache
                        // If a match is found, add it to reputationResults and do not add it to the
                        // list that gets sent to google
                        Reputation resultFromFullCache = Reputation.None;
                        if (true == IsEntryInFullHashCache(hashedUrl, out resultFromFullCache))
                        {
                            reputationResults[index] |= resultFromFullCache;
                        }
                        else
                        {
                            Reputation resultFromCache = Reputation.None;
                            CompareEntry(ref resultFromCache, hashedUrl);

                            // If hash is in a blacklist and not in the respective whitelist, then look up full hash on server
                            // Note: Whitelist checking is currently disabled
                            if (((0 != (resultFromCache & Reputation.MalwareBlackList))) ||
                                ((0 != (resultFromCache & Reputation.PhishBlackList))))
                            {
                                hashList.Add(hashedUrl);
                                hashedUrlList[index].Add(hashedUrl);
                            }
                        }
                    }
                }

                List<byte[]> responses = new List<byte[]>();

                // Make a request to the google server. Requests are grouped to minimize the number of round trips.
                if (0 < hashList.Count)
                {
                    // Note, there is an undocumented Google limit of 1000 hash prefixes per request.
                    // Split requests into groups of 1000.
                    for (int group = 0; group < hashList.Count; group += 1000)
                    {
                        int cGroup = hashList.Count - group;
                        if (cGroup > 1000)
                        {
                            cGroup = 1000;
                        }

                        int cbHashSize = 4;
                        int cbHashes = cGroup * cbHashSize;
                        int cbHeader_1 = 2;
                        int cbHeader_2 = cbHashes.ToString().Length;
                        int cbHeader_3 = 1;
                        int cbHeaderFull = cbHeader_1 + cbHeader_2 + cbHeader_3;

                        // Build the request for the full hashes
                        byte[] request = new byte[cbHeader_1 + cbHeader_2 + cbHeader_3 + cbHashes];
                        request[0] = (byte)'4';
                        request[1] = (byte)':';
                        for (int index = 0; cbHeader_2 > index; index++)
                        {
                            request[cbHeader_1 + index] = (byte)cbHashes.ToString()[index];
                        }
                        request[cbHeader_1 + cbHeader_2] = (byte)'\n';

                        for (int index = 0; cGroup > index; index++)
                        {
                            Buffer.BlockCopy(hashList[index + group], 0, request, cbHeaderFull + (index * cbHashSize), cbHashSize);
                        }

                        // Send the request for the full hashes
                        byte[] response = GetByteArrayFromUrl(MakeSafebrowseCommandUrl(CommandGetHash), request);
                        responses.Add(response);
                    }

                    // Iterate through each response (a response is the result of a round-trip to google server)
                    foreach (byte[] response in responses)
                    {
                        // Iterate through the response for listname/hash key responses (note, there can be more than one hash key for each listname entry)
                        int indexResponse = 0;
                        while (response.Length > indexResponse)
                        {
                            int indexLength = 0;
                            while ('\n' != response[indexResponse + indexLength]) indexLength++;
                            // The tag is the ASCII portion of each entry (listname/hashes) in the response.
                            // There can be multiple tags in one response
                            string tag = System.Text.Encoding.ASCII.GetString(response, indexResponse, indexLength);
                            // The list is the listname for the subsequent hashes.
                            // For example: The SHA256 hash that follows was found in the malware list
                            string list = tag.Substring(0, tag.IndexOf(":"));
                            // The chunk is the chunk number that the associated SHA256 relates to
                            // (for example, you should find the respective hash prefix in this chunk)
                            string chunk = tag.Substring(tag.IndexOf(":") + 1, tag.LastIndexOf(":") - tag.IndexOf(":") - 1);
                            // The szHashLength is the count of bytes of the hashes for this entry.
                            // This must be a multiple of 32 (the size of a SHA256 hash)
                            string szHashLength = tag.Substring(tag.LastIndexOf(":") + 1, tag.Length - tag.LastIndexOf(":") - 1);
                            int hashLength = Convert.ToInt32(szHashLength);
                            if (0 != (hashLength % cbSha256))
                            {
                                // ASSERT: Invalid SHA256 Hash found in response
                                break;
                            }

                            indexResponse += indexLength + 1;

                            // Iterate through each SHA256 hash in this server response entry
                            int indexHashGroup = 0;
                            for (indexHashGroup = 0; hashLength > indexHashGroup; indexHashGroup += cbSha256)
                            {
                                // Iterate through each hashed Url list in hashedUrlList
                                for (int index = 0; index < hashedUrlList.Count; index++)
                                {
                                    // Iterate through each hashed Url entry in the hashed Url list in hashedUrlList (this list might be empty)
                                    foreach (byte[] hash in hashedUrlList[index])
                                    {
                                        // Compare the server response SHA256 hash with the one we computed from the Url
                                        bool fMatch = true;
                                        for (int indexHash = 0; cbSha256 > indexHash; indexHash++)
                                        {
                                            if (hash[indexHash] != response[indexResponse + indexHashGroup + indexHash])
                                            {
                                                fMatch = false;
                                                break;
                                            }
                                        }

                                        // If there's a match, then mark the resultFromServer with the associated list.
                                        if (true == fMatch)
                                        {
                                            if (ListMalware == list)
                                            {
                                                reputationResults[index] |= Reputation.MalwareBlackList;
                                            }
                                            else if (ListPhish == list)
                                            {
                                                reputationResults[index] |= Reputation.PhishBlackList;
                                            }

                                            // Add the full hash to the fullHashCache, so if the hash comes up again, we don't
                                            // need to round-trip to google to determine its reputation.
                                            UpdateFullHashCache(hash, ((list == ListMalware) ? Reputation.MalwareBlackList : Reputation.PhishBlackList));
                                        }
                                    }
                                }
                            }

                            indexResponse += indexHashGroup;
                        }
                    }
                }

                return reputationResults;
            }

            // If the hash exists in the fullHashCache, then this returns true and the associated reputation
            // If it does not exist, then it returns false and Reputation.None
            private bool IsEntryInFullHashCache(byte[] hash, out Reputation reputationFromCache)
            {
                bool fFound = false;
                reputationFromCache = Reputation.None;

                for (int index = 0; index < fullHashCache.Count; index++)
                {
                    // Compare the byte array of the hash
                    if (true == fullHashCache[index].hash.SequenceEqual(hash))
                    {
                        // There was a match. Extract the stored reputation and delete the entry
                        reputationFromCache = fullHashCache[index].reputation;
                        fFound = true;
                        break;
                    }
                }

                return fFound;
            }

            // This walks through fullHashCache, and if it finds the hash, this changes that entry to include the new reputation value
            private void UpdateFullHashCache(byte[] hash, Reputation newReputation)
            {
                Reputation reputationFromCache = Reputation.None;
                for (int index = 0; index < fullHashCache.Count; index++)
                {
                    // Compare the byte array of the hash
                    if (true == fullHashCache[index].hash.SequenceEqual(hash))
                    {
                        // There was a match. Extract the stored reputation and delete the entry
                        reputationFromCache = fullHashCache[index].reputation;
                        fullHashCache.RemoveAt(index);
                        break;
                    }
                }

                // Create a new entry with the merged reputation value
                byte[] newHash = (byte[])hash.Clone();
                fullHashCache.Add(new HashAndReputation(newHash, (reputationFromCache | newReputation)));
            }

            // Returns true if there is a match to a cached list (e.g. the entry is malicious)
            static private void CompareEntry(ref Reputation result, byte[] compareHash)
            {
                // The following code uses the quickLookupCache
                UInt32 hash = GetLittleEndianUintFromByteArray(ref compareHash);
                if (quickLookupCache.ContainsKey(hash))
                {
                    result = quickLookupCache[hash];
                }
                else
                {
                    result = Reputation.None;
                }


                /*
                 * The following code uses the master lists. This code is much slower than using the quickLookupCache,
                 * but it is compatible with whitelists and hash prefixes that aren't 32 bits.
                 * To use this code you'll need to change this method signature to include ref MasterList[] masterLists
                {
                    // For each master list (e.g. malware blacklist)
                    // Shortcut: Skipping checks on whitelists.
                    for (int index = 0; masterLists.Length > index; index += 2)
                    {
                        if (0 != (result & (Reputation)(0x1 << (index / 2))))
                        {
                            continue;
                        }

                        // Iterate through all chunks in the list
                        foreach (KeyValuePair<uint, Chunk> kvp in masterLists[index].e)
                        {
                            // In each chunk, check for a match
                            uint compareHashNumber = GetLittleEndianUintFromByteArray(ref compareHash);
                            if (true == kvp.Value.e.ContainsKey(compareHashNumber))
                            {
                                result |= (Reputation)(0x1 << (index / 2));
                                break;
                            }
                        }
                    }
                }
                 * */
            }

            // Calculate SHA256 hash from input
            static private byte[] MakeSHA256Hash(string input)
            {
                SHA256 sha256 = new SHA256Managed();
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hash = sha256.ComputeHash(inputBytes);

                return hash;
            }

            // Removes any character from a string that is of value >0xFF
            static public string RemoveHighOrderByteCharacters(string input)
            {
                for (int index = input.Length - 1; 0 <= index; index--)
                {
                    if (0xFF < input[index])
                    {
                        input = input.Remove(index, 1);
                    }
                }

                return input;
            }

            // Canonicalize an input based on the safe browsing API rules. Note, this returns a list of strings (maximum 30)
            static public List<string> Canonicalize(string input)
            {
                // Pre-cleaning - remove any character that is value >0xFF (these cause exceptions)
                string cleanedUrl = RemoveHighOrderByteCharacters(input);

                // Step 1: URL-unescape until there are no more hex-encodings
                string passCurrent = cleanedUrl;
                string passPrevious = "";
                bool fUrlIsValid = true;
                while (passPrevious != passCurrent)
                {
                    passPrevious = passCurrent;
                    try
                    {
                        passCurrent = Uri.UnescapeDataString(passPrevious);
                    }
                    catch
                    {
                        fUrlIsValid = false;
                    }
                }

                if (false == fUrlIsValid)
                {
                    return new List<string>();
                }

                // Mid-cleaning - remove any character that is value >0xFF (these cause exceptions)
                passCurrent = RemoveHighOrderByteCharacters(passCurrent);

                // Locate the end of the scheme/beginning of the host
                bool fSchemeFound = false;
                int index = 0;
                for (index = 0; index < passCurrent.Length - 2; index++)
                {
                    if ((':' == passCurrent[index]) &&
                        ('/' == passCurrent[index + 1]) &&
                        ('/' == passCurrent[index + 2]))
                    {
                        index += 3;
                        fSchemeFound = true;
                        break;
                    }
                }

                // Check if the scheme was malformed (e.g. it was http:/)
                if (false == fSchemeFound)
                {
                    return new List<string>();
                }

                // Remove extra slashes in the scheme (e.g. it was http://///something)
                while ((passCurrent.Length > index) && ('/' == passCurrent[index]))
                {
                    passCurrent = passCurrent.Remove(index, 1);
                }

                // Check if the URL was blank (e.g. the string passed in was "http://").
                if (passCurrent.Length == index)
                {
                    return new List<string>();
                }

                // Copy out the scheme
                string scheme = passCurrent.Remove(index);
                string remainder = passCurrent.Remove(0, index); // hostPortPathQueryFragment

                // Locate the end of the host/beginning of the port OR Path
                bool fContainsPort = false;
                bool fContainsPath = false;
                for (index = 0; remainder.Length > index; index++)
                {
                    if ((':' == remainder[index]) && (remainder.Length > index + 1))
                    {
                        fContainsPort = true;
                        break;
                    }
                    else if ('/' == remainder[index])
                    {
                        if (remainder.Length > index + 1)
                        {
                            fContainsPath = true;
                            break;
                        }
                        else // The URL ends with a slash, which needs to be stripped out of the host name
                        {
                            break;
                        }
                    }
                }

                string host = remainder;
                if (remainder.Length > index) host = remainder.Remove(index);
                // Check if the host is blank (this could occur in an url like http://:80/hello )
                if ("" == host)
                {
                    return new List<string>();
                }
                index++;

                // Copy out the port (if it exists)
                string port = "";
                if (true == fContainsPort)
                {
                    int beginPort = index;
                    for (; remainder.Length > index; index++)
                    {
                        if ('/' == remainder[index]) break;
                    }
                    if (index < remainder.Length)
                    {
                        port = (remainder.Remove(index)).Remove(0, beginPort);
                    }
                    else
                    {
                        port = remainder.Remove(0, beginPort);
                    }
                    if (remainder.Length > index + 1)
                    {
                        fContainsPath = true;
                        index++;
                    }
                }

                // Copy out the path, query, and fragment
                string pathQueryFragment = "";
                string path = "/";
                string queryFragment = "";
                if (true == fContainsPath)
                {
                    pathQueryFragment = remainder.Remove(0, index - 1);

                    // Determine whether there's a query and/or fragment
                    bool fContainsQueryOrFragment = false;
                    for (index = 0; pathQueryFragment.Length > index; index++)
                    {
                        if (('?' == pathQueryFragment[index]) || ('#' == pathQueryFragment[index]))
                        {
                            fContainsQueryOrFragment = true;
                            break;
                        }
                    }

                    if (true == fContainsQueryOrFragment)
                    {
                        for (index = 0; pathQueryFragment.Length > index; index++)
                        {
                            if (('?' == pathQueryFragment[index]) || ('#' == pathQueryFragment[index])) break;
                        }
                        queryFragment = pathQueryFragment.Remove(0, index);
                        path = pathQueryFragment.Remove(index);
                    }
                    else
                    {
                        path = pathQueryFragment;
                    }
                }

                // Step 2: Hostname Processing
                // Remove leading dots
                index = 0;
                while ((index < host.Length) && ('.' == host[index])) index++;
                // If the host is malformed (such as '...') return an empty list
                if (index == host.Length) return new List<string>(); 
                if (0 < index) host = host.Remove(0, index);

                // Remove trailing dots
                index = host.Length - 1;
                while ('.' == host[index]) index--;
                if (host.Length - 1 > index) host = host.Remove(index + 1);

                // Replace consecutive dots with a single dot
                for (index = 0; host.Length > index; index++)
                {
                    if ('.' == host[index]) // If a dot is found
                    {
                        // Delete all subsequent dots (since trailing dots are processed earlier, this is guaranteed safe)
                        while ('.' == host[index + 1]) host = host.Remove(index + 1, 1);
                    }
                }

                // If the hostname can be parsed as an IP address, it should be normalized to 4 dot-separated decimal values.
                // The client should handle any legal IP address encoding, including octal, hex, and fewer than 4 components.
                bool fHostnameIsIPNumber = false; // Temporary hardcode
                string hostnameAsIPNumber = CleanHostnameIfIPNumber(host);
                if ("" != hostnameAsIPNumber)
                {
                    host = hostnameAsIPNumber;
                    fHostnameIsIPNumber = true;
                }

                // Lowercase the whole string
                host = host.ToLower();
                // Re-escape hostname (escaped characters are uppercase)
                host = Uri.EscapeDataString(host);

                if (true == fContainsPath)
                {
                    // Step 3: Path processing
                    // Replace "/./" with "/"
                    for (index = 0; path.Length > index; index++)
                    {
                        if (('/' == path[index]) &&
                            (path.Length > index + 2) &&
                            ('.' == path[index + 1]) &&
                            ('/' == path[index + 2]))
                        {
                            path = path.Remove(index + 1, 2);
                            index--; // In case there are multiple /././ in the path
                        }
                    }

                    // Remove all double slashes
                    for (index = 0; path.Length > index; index++)
                    {
                        if (('/' == path[index]) &&
                            (path.Length > index + 1) &&
                            ('/' == path[index + 1]))
                        {
                            path = path.Remove(index + 1, 1);
                            index--; // In case there are multiple /// in the path
                        }
                    }

                    // Remove /../ and the path before it
                    // Keep track of how many legit paths there are (you cannot remove a previous path if you're at root)
                    int pathDepth = 0;
                    for (index = 0; path.Length > index; index++)
                    {
                        if ('/' == path[index]) // This signifies a new path
                        {
                            pathDepth++;
                            // Check to see if the next path is actually a back-one-directory
                            if ((path.Length > index + 3) &&
                                ('.' == path[index + 1]) &&
                                ('.' == path[index + 2]) &&
                                ('/' == path[index + 3]))
                            {
                                // Don't count this path in the depth
                                pathDepth--;
                                // Remove the "/../" itself
                                path = path.Remove(index + 1, 3);
                                if (pathDepth > 0)
                                {
                                    // Rid one path back
                                    int end = index;
                                    index--;
                                    // Search for the beginning of the previous path
                                    while ('/' != path[index]) index--;
                                    // Remove the previous path
                                    path = path.Remove(index, end - index);
                                    // Decrement the path depth
                                    pathDepth--;
                                }
                                // In case there are multiple /../../ in the path
                                index--;
                            }
                        }
                    }
                }

                // Re-escape all characters in the path and query/fragment which are <=32 or >=127, or the % character (escape characters are uppercase)
                path = ReEscape(path);
                queryFragment = ReEscape(queryFragment);

                // Do not include scheme or port in result

                List<string> hosts = MakeHostList(fHostnameIsIPNumber, host);

                List<string> paths = MakePathList(path, queryFragment);

                List<string> results = new List<string>();
                for (int indexHosts = 0; hosts.Count > indexHosts; indexHosts++)
                {
                    for (int indexPaths = 0; paths.Count > indexPaths; indexPaths++)
                    {
                        results.Add(hosts[indexHosts] + paths[indexPaths]);
                    }
                }

                return results;
            }

            // Re escape the input string
            static private string ReEscape(string input)
            {
                string result = input;
                for (int index = 0; index < result.Length; index++)
                {
                    if ((32 >= result[index]) ||
                        (127 <= result[index]) ||
                        ('%' == result[index]))
                    {
                        string escape = Uri.HexEscape(result[index]);
                        result = result.Remove(index) + escape + result.Remove(0, index + 1);
                        index += escape.Length - 1;
                    }
                }
                return result;
            }

            // Make hostlist (array of strings)
            // For example, if the host is a.b.c.d.e.f.g, then we will try a.b.c.d.e.f.g, and also
            // up to 4 hostnames formed by starting with the last 5 components and successively
            // removing the leading component. The top-level domain can be skipped.
            // Specifically, c.d.e.f.g, d.e.f.g, e.f.g, and f.g (not b.c.d.e.f.g)
            static private List<string> MakeHostList(bool fIsHostnameAnIPNumber, string host)
            {
                List<string> hosts = new List<string>();
                // Add the original host, regardless whether it is an IP number or name
                hosts.Add(host);
                if (false == fIsHostnameAnIPNumber)
                {
                    // Compute how many components are there
                    int cDots = 0;
                    int indexHost = 0;
                    for (indexHost = host.Length - 1; 0 <= indexHost; indexHost--)
                    {
                        // Dots separate components
                        if ('.' == host[indexHost])
                        {
                            cDots++;
                            if (5 == cDots)
                            {
                                cDots--;
                                // Max 4 dots (5 components)
                                break;
                            }
                        }
                    }

                    indexHost++;
                    // index now pointing to the first character of the (max) 5th component of the hostname (e.g. pointing to the 'c' of a.b.c.d.e.f.g)
                    // whether it's at the beginning of the string, or somewhere in the middle

                    if (0 != indexHost)
                    {
                        // If we're not at the beginning of the string, add this host.
                        // If we were at the beginning, this would be a duplicate of the full host name, which was added above.
                        hosts.Add(host.Remove(0, indexHost));
                    }
                    for (int indexDots = 0; (cDots - 1) > indexDots; indexDots++)
                    {
                        while ('.' != host[indexHost++]) ; // Loop here until the next dot is found
                        hosts.Add(host.Remove(0, indexHost));
                    }
                }

                // hosts now contains a list of all the possible hosts to try

                return hosts;
            }

            // Make pathlist (array of strings)
            // Include the exact path of the url, including query parameters (if exists)
            // Include the exact path of the url, without query parameters
            // Include the 4 paths formed by starting at the root (/) and successively
            // appending path components, including a trailing slash. 
            static private List<string> MakePathList(string path, string queryFragment)
            {
                List<string> paths = new List<string>();
                if ("" != queryFragment)
                {
                    paths.Add(path + queryFragment);
                }
                if ("/" != path)
                {
                    paths.Add(path);
                }
                paths.Add("/");

                // Count up to 3 additional slashes, and add those portions as a path
                int cSlashes = 0;
                int indexPath = 0;

                // First character is a slash, so start counting at the 2nd character
                // Checking to path.Length - 1 so we don't count the last character as a slash (if it is one)
                for (indexPath = 1; path.Length - 1 > indexPath; indexPath++)
                {
                    // Slashes separate paths
                    if ('/' == path[indexPath])
                    {
                        cSlashes++;
                        if (3 == cSlashes) break;
                    }
                }

                indexPath = 1;
                for (int indexSlashes = 0; cSlashes > indexSlashes; indexSlashes++)
                {
                    // Loop here until the next slash is found
                    while ('/' != path[indexPath++]) ;
                    paths.Add(path.Remove(indexPath));
                }

                return paths;
            }

            // Convert the hostname to a cleaned IP number if it is a number
            // Rules
            // 1. Ensure there are 1 to 4 components of the hostname. Track quantity.
            // 2. Ensure each component is a number
            // 3. Convert each number to a variable
            // 4. Truncate all but last variable to 1 byte
            // 5. Truncate last variable to 1 byte if 4 components, 2 if 3, 3 if 2, and 4 bytes if 1 component
            //      Then start filling the IP components from the back to the front.
            // 6. Convert each decimal to string and return cleaned hostname
            // If the return is "", that means the hostname is not an IP number
            static private string CleanHostnameIfIPNumber(string input)
            {
                if ("" == input)
                {
                    // Bogus hostname passed in
                    return "";
                }

                // Ensure there are 1 to 4 components in the hostname.
                int totalComponents = 1;
                int indexInput = 0;
                for (indexInput = 0; input.Length > indexInput; indexInput++)
                {
                    if ('.' == input[indexInput])
                    {
                        totalComponents++;
                        // 4 dots means 5 or more components
                        if (5 == totalComponents)
                        {
                            // The hostname is not an IP number
                            return "";
                        }
                    }
                }

                if ('.' == input[input.Length - 1])
                {
                    // The last dot was actually the end of the string. This hostname is not an IP address
                    return "";
                }

                long[] components = new long[4];

                // Ensure each component is a number, and convert it to a variable
                int placeholderBegin = 0;
                int indexComponents = 0;
                indexInput = 0;
                for (indexComponents = 0; totalComponents > indexComponents; indexComponents++)
                {
                    // Find the end of the current component
                    while ((input.Length > indexInput) && ('.' != input[indexInput])) indexInput++;

                    string stringComponent;
                    if (input.Length > indexInput)
                    {
                        stringComponent = input.Remove(indexInput).Remove(0, placeholderBegin);
                    }
                    else
                    {
                        stringComponent = input.Remove(0, placeholderBegin);
                    }

                    if ("" == stringComponent)
                    {
                        // The component is blank. This hostname is not an IP address
                        return "";
                    }

                    // Pass in just the component in question
                    if (false == ConvertComponentToNumber(stringComponent, out components[indexComponents]))
                    {
                        // The hostname is not an IP address
                        return "";
                    }

                    indexInput++;
                    placeholderBegin = indexInput;
                }

                // Truncate all but last variable to 1 byte
                for (indexComponents = 0; totalComponents - 1 > indexComponents; indexComponents++)
                {
                    components[indexComponents] &= 0xFFL;
                }

                // Truncate last variable to 1 byte if 4 components, 2 if 3, 3 if 2, and 4 bytes if 1 component
                // Then start filling the IP components from the back to the front.
                long fillComponent = components[totalComponents - 1];
                for (indexComponents = totalComponents - 1; 4 > indexComponents; indexComponents++)
                {
                    int shift = (3 - indexComponents) * 8;
                    components[indexComponents] = ((fillComponent & (0xFFL << shift))) >> shift;
                }

                // The above 'for loop' is functionally equivalent to the below statements
                // EXAMPLE: if (1 >= totalComponents) components[0] = ((fillComponent & (0xFFL << 24))) >> 24;
                // EXAMPLE: if (2 >= totalComponents) components[1] = ((fillComponent & (0xFFL << 16))) >> 16;
                // EXAMPLE: if (3 >= totalComponents) components[2] = ((fillComponent & (0xFFL << 8))) >> 8;
                // EXAMPLE: if (4 >= totalComponents) components[3] = ((fillComponent & (0xFFL << 0))) >> 0;

                // Convert each decimal to string and return cleaned hostname
                string hostname = components[0].ToString() + "." + components[1].ToString() + "." + components[2].ToString() + "." + components[3].ToString();

                return hostname;
            }

            private enum NumberType { Invalid, Octal, Decimal, Hex };

            static private bool ConvertComponentToNumber(string input, out long output)
            {
                output = 0;

                if ("" == input)
                {
                    // Empty string is not a number
                    return false;
                }

                NumberType numberType = NumberType.Invalid;

                // Determine what kind of number it is (octal, decimal, or hex)
                if ('0' == input[0])
                {
                    // Is it the decimal number 0?
                    if (1 == input.Length)
                    {
                        // Value is supposed to be 0
                        output = 0;
                        // Short circuit
                        return true;
                    }

                    // Is it hex?
                    if (('x' == input[1]) || ('X' == input[1]))
                    {
                        if (3 <= input.Length)
                        {
                            numberType = NumberType.Hex;
                        }
                        else
                        {
                            // It is a malformed hex value (e.g. it is just "0x")
                            return false;
                        }
                    }
                    else
                    {
                        numberType = NumberType.Octal;
                    }
                }
                else if (('1' <= input[0]) && ('9' >= input[0]))
                {
                    numberType = NumberType.Decimal;
                }
                else
                {
                    // It's not a number
                    return false;
                }

                // We now know what type of number it's trying to be. Determine what it is based on that fact
                switch (numberType)
                {
                    case NumberType.Octal:
                        for (int index = 1; input.Length > index; index++)
                        {
                            if (('0' > input[index]) || ('7' < input[index]))
                            {
                                // It's not a valid octal number
                                return false;
                            }
                        }
                        output = Convert.ToInt64(input.Remove(0, 1), 8);
                        break;
                    case NumberType.Decimal:
                        for (int index = 1; input.Length > index; index++)
                        {
                            if (('0' > input[index]) || ('9' < input[index]))
                            {
                                // It's not a valid decimal number
                                return false;
                            }
                        }
                        output = Convert.ToInt64(input, 10);
                        break;
                    case NumberType.Hex:
                        for (int index = 2; input.Length > index; index++)
                        {
                            // If it's NOT between 0-9, a-f, or A-F
                            if (!(('0' <= input[index]) && ('9' >= input[index]) ||
                                   ('a' <= input[index]) && ('f' >= input[index]) ||
                                   ('A' <= input[index]) && ('F' >= input[index])))
                            {
                                // It's not a valid hex number
                                return false;
                            }
                        }
                        output = Convert.ToInt64(input.Remove(0, 2), 16);
                        break;
                }

                return true;
            }
        }

        /*******************************************************************************
         * This class is removed because it is not needed by SIU - we refresh every 30 *
         * minutes as a scheduled task - we don't want a process to run forever that   *
         * would be leaky the way we want to use it.  So to prevent user from trying   *
         * to do this this class is removed.                                           *
         *******************************************************************************/

        /*protected class SafeBrowsingWorker
        {
            public MasterList[] masterLists;

            public void DoWork()
            {
                // Every 1/2 hour, retrieve the latest safe browsing list from the server
                for (; ; )
                {
                    try
                    {
                        SafeBrowsing.UpdateList(masterLists);
                    }
                    catch
                    {
                        if (eventLog != null) eventLog.WriteEntry("Failed to retrieve new lists from Safe Browsing server.", EventLogEntryType.Warning);
                    }

                    Thread.Sleep(PollIntervalInSeconds * 1000);
                }
            }
        }*/
    }
}
