using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("İsim alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("İsim en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("En fazla 50 karakter olabilir.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("Açıklama en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Açıklama en fazla 50 karakter olabilir.");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Fiyat alanı boş geçilemez.")
                .GreaterThan(0).WithMessage("Fiyat sıfırdan küçük olamaz.");

            RuleFor(x => x.Stock)
                .NotEmpty().WithMessage("Stok alanı boş geçilemez.")
                .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Kategori alanı boş geçilemez.");

            RuleFor(x => x.SubcategoryId)
                .NotEmpty().WithMessage("Alt kategori alanı boş geçilemez.");
        }
    }
}
