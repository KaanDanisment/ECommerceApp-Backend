using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class SubcategoryCreateDtoValidator : AbstractValidator<SubcategoryCreateDto>
    {
        public SubcategoryCreateDtoValidator()
        {
            RuleFor(subcategory => subcategory.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Alt kategori adı boş geçilemez")
                .MinimumLength(3).WithMessage("Kategori adı en az 3 karakter olabilir")
                .MaximumLength(10).WithMessage("Kategori adı en fazla 20 karakter olabilir");

            RuleFor(subcategory => subcategory.CategoryId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Kategori alanı boş geçilemez");
        }
    }
}
