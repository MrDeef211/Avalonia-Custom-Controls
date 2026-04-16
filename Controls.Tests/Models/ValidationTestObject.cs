using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit.Sdk;

namespace Controls.Tests.Models
{
    public class ValidationTestObject
    {
        [Range(1, 100, ErrorMessage = "Значение должно быть от 1 до 100")]
        public int Number { get; set; } = 50;

        [StringLength(5, ErrorMessage = "Максимум 5 символов")]
        public string ShortString { get; set; } = "abc";

        [Required(ErrorMessage = "Обязательное поле")]
        public string RequiredString { get; set; } = "initial";
    }
}
