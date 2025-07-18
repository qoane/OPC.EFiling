namespace OPC.EFiling.Domain.Entities
{
    public class UploadedFile
    {
        public int UploadedFileID { get; set; }
        public int DocumentID { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public int UploadedBy { get; set; }
        public DateTime DateUploaded { get; set; }
    }
}