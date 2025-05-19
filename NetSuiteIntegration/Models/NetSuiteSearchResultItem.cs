using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetSuiteIntegration.Models
{
    [Keyless]
    public class NetSuiteSearchResultItem
    {
        public ICollection<NetSuiteLink>? Links { get; set; }
        public string? ID { get; set; }

        [JsonIgnore]
        public int? IDFromIDAndLink
        {
            get
            {
                if (ID != null)
                {
                    if (int.TryParse(ID, out int id))
                    {
                        return id;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (Links != null && Links.Count > 0)
                {
                    string? url = Links.FirstOrDefault()?.Href;
                    string[] parts = url?.Split('/') ?? new string[0];
                    string? id = parts?.LastOrDefault();
                    if (id != null)
                    {
                        if (int.TryParse(id, out int idFromLink))
                        {
                            return idFromLink;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
