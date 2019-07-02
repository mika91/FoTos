//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace photo_tos_maton.uploader
//{
//    public class clsResponseRootObject
//    {
//        public List<MediaItem> mediaItems { get; set; }
//        public string nextPageToken { get; set; }
//    }

//    public class MediaItem
//    {
//        public string id { get; set; }
//        public string productUrl { get; set; }
//        public string baseUrl { get; set; }
//        public string mimeType { get; set; }
//        public MediaMetadata mediaMetadata { get; set; }
//        public string filename { get; set; }
//    }

//    public class MediaMetadata
//    {
//        public DateTime creationTime { get; set; }
//        public string width { get; set; }
//        public string height { get; set; }
//        public Photo photo { get; set; }
//    }

//    public class Photo
//    {
//        public string cameraMake { get; set; }
//        public string cameraModel { get; set; }
//        public double focalLength { get; set; }
//        public double apertureFNumber { get; set; }
//        public int isoEquivalent { get; set; }
//    }
    
//    #region album

//    public class Album
//    {
//        public String id { get; set; }
//        public String title { get; set; }
//        public String productUrl { get; set; }
//        public Boolean isWriteable { get; set; }

//        public ShareInfo shareInfo { get; set; }

//        public String mediaItemsCount { get; set; }

//        public String coverPhotoBaseUrl { get; set; }

//        public String coverPhotoMediaItemId { get; set; }
//    }

//    public class ShareInfo
//    {
//        public SharedAlbumOptions sharedAlbumOptions { get;set;}
//        public String shareableUrl { get; set; }

//        public String shareToken { get; set; }

//        public Boolean isJoined { get; set; }
//    }

//    public class SharedAlbumOptions
//    {
//        public Boolean isCollaborative { get; set; }
//        public Boolean isCommentable {get;set;}
//    }

//    #endregion
//}
