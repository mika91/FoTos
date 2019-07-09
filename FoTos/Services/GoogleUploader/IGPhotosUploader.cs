using System;
using System.Threading.Tasks;

namespace FoTos.Services.GoogleUploader
{
    public interface IGPhotosUploader
    {
        String UploadDirectory { get;  }

        Task Upload(String filename);
    }
}
