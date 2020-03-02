using System.Collections.Generic;

namespace FileApi.V1.Model
{
    public class GetFoldersResponse
    {
        public IEnumerable<string> Paths { get; set; }
    }
}