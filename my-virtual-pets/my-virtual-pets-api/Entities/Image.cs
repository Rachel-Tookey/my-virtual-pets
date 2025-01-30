﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace my_virtual_pets_api.Entities
{
    public class Image
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("imageobj")]
        [Column(TypeName = "byte[]")]
        public byte[] ImageObj { get; set; }

    }
}
