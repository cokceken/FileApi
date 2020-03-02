using System.ComponentModel.DataAnnotations;

namespace FileApi.V1.Model
{
    public class GetFoldersRequest
    {
        /// <summary>
        /// Path to the folder to be analyzed
        /// </summary>
        [Required]
        public string Path { get; set; }
        /// <summary>
        /// Count of the sub-folders to be returned
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Count { get; set; } = 5;
        /// <summary>
        /// When set, ignores access errors on files and folders
        /// </summary>
        public bool SuppressAccessErrors { get; set; } = false;
    }
}