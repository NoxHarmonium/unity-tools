namespace UnityTools.Utils
{
    using System;
    using System.IO;

    using UnityTools.Threading;

    public static class Extensions
    {
        #region Methods

        public static UnityTask CopyToAsync(this Stream inStream, Stream outStream, int bufferSize = 4096)
        {
            return new UnityTask(task => CopyTo(inStream, outStream, bufferSize));
        }

        public static void CopyToSync(this Stream inStream, Stream outStream, int bufferSize = 4096)
        {
            CopyTo(inStream, outStream, bufferSize)(null);
        }

        private static Action<UnityTask> CopyTo(Stream inStream, Stream outStream, int bufferSize = 4096)
        {
            return task =>
            {
                byte[] buffer = new byte[4096];
                int offset = 0;
                int bytesRead;

                while((bytesRead = inStream.Read(buffer, offset, bufferSize)) > 0)
                {
                    outStream.Write(buffer, offset, bytesRead);
                }
                if (task != null)
                {
                    task.Resolve();
                }
            };
        }

        #endregion Methods
    }
}