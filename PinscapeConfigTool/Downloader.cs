using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using StringUtils;

// Downloader - checks for updates to online components (e.g., the 
// KL25Z firmware .bin file) and downloads as needed.
namespace PinscapeConfigTool
{
    class Downloader
    {
        public Downloader()
        {
        }

        // status callback interface
        public interface IStatus 
        {
            // intermediate status report
            void Progress(String htmlMessage, bool done, bool downloadsFound);
        }

        public enum Status
        {
            CheckFailed,
            NoUpdate,
            DownloadFailed,
            DownloadDone
        };

        // Check for a new version of the given file.  If a new version is available,
        // attempt to download it.  Returns a status indication.
        public Status CheckForUpdates(String desc, String url, String localName, IStatus status, out String errmsg)
        {
            // send a status update
            Action<String, bool> Progress = (String msg, bool downloadFound) => 
            {
                // add an implicit "desc: " if it's not there
                if (msg.IndexOf("{0}") < 0)
                    msg = "{0}: " + msg;

                // if the desc field is at the very start of the message, capitalize the desc string
                String adesc = desc;
                if (msg.StartsWith("{0}"))
                    adesc = adesc.Substring(0, 1).ToUpper() + adesc.Substring(1);

                // format the message
                status.Progress(String.Format(msg, adesc), false, downloadFound); 
            };

            // presume success
            Status result = Status.DownloadDone;
            errmsg = null;

            // get the full path to the local file and the etag file
            String localPath = Path.Combine(Program.dlFolder, localName);
            String etagPath = localPath + ".etag";

            // presume we'll fetch a new version
            bool fetch = true;

            // if the local file exists, check to see if we already have the latest
            if (File.Exists(etagPath))
            {
                // update our status
                Progress("Checking for updates to {0}", false);
                try
                {
                    // get the remote file metadata via an HTTP HEAD request
                    WebRequest req = WebRequest.Create(url);
                    req.Method = "HEAD";
                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        if (resp.StatusCode == HttpStatusCode.OK
                            && resp.Headers["ETag"] != null)
                        {
                            // If the old ETag matches the one we just got, we have the latest
                            // file from the server, so no update is required
                            String oldTag = File.ReadAllText(etagPath);
                            if (oldTag == resp.Headers["ETag"])
                            {
                                fetch = false;
                                result = Status.NoUpdate;
                            }
                        }
                        else
                        {
                            // error checking for an update
                            result = Status.CheckFailed;
                        }

                        // done with the response
                        resp.Close();
                    }
                }
                catch (Exception)
                {
                    result = Status.CheckFailed;
                }
            }

            // fetch the new file if necessary
            if (fetch)
            {
                // update our status
                Progress("New {0} version available - requesting file", true);

                // request the remote file contents
                WebRequest req = WebRequest.Create(url);
                HttpWebResponse resp = null;
                try
                {
                    using (resp = (HttpWebResponse)req.GetResponse())
                    {
                        if (resp == null)
                        {
                            result = Status.DownloadFailed;
                            errmsg = "No HTTP response";
                        }
                        else if (resp.StatusCode == HttpStatusCode.OK)
                        {
                            // update status
                            Progress("Downloading new {0} version", true);

                            // got it - copy the response stream to the local file
                            Stream streamIn = resp.GetResponseStream();
                            Stream streamOut = new FileStream(localPath, FileMode.Create);
                            streamIn.CopyTo(streamOut);

                            // close files
                            streamIn.Close();
                            streamOut.Close();

                            // If the response has an ETag, write it to the local ETag
                            // file.  This will let us check next time to see if our
                            // cached copy is the same as the one on the server.  If
                            // there's no ETag in the response, delete any existing ETag
                            // file, since we don't have cache data for next time.
                            String etag;
                            if ((etag = resp.Headers["ETag"]) != null)
                                File.WriteAllText(etagPath, etag);
                            else if (File.Exists(etagPath))
                                File.Delete(etagPath);
                        }
                        else
                        {
                            // report the error
                            result = Status.DownloadFailed;
                            errmsg = "HTTP error: " + resp.StatusDescription;
                        }

                        // done with the response
                        resp.Close();
                    }
                }
                catch (WebException ex)
                {
                    result = Status.DownloadFailed;
                    resp = (HttpWebResponse)ex.Response;
                    errmsg = "HTTP error: " + resp.StatusDescription;
                }
            }

            // update status
            Progress("{0}: Done", fetch);

            // return the result
            return result;
        }
    }
}
