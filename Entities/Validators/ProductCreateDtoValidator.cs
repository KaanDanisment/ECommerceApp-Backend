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
            RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("İsim alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("İsim en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("En fazla 50 karakter olabilir.");

            RuleFor(x => x.Color).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Renk alanı boş geçilemez.")
                .Matches("^[a-zA-Z]*$").WithMessage("Renk sadece harf içerebilir.");

            RuleFor(x => x.Description).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Açıklama alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("Açıklama en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Açıklama en fazla 50 karakter olabilir.");

            RuleFor(x => x.Price).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Fiyat alanı boş geçilemez.")
                .GreaterThan(0).WithMessage("Fiyat sıfırdan küçük olamaz.");

            RuleFor(x => x.Stock).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Stok alanı boş geçilemez.")
                .GreaterThanOrEqualTo(0).WithMessage("Stok negatif olamaz.");

            RuleFor(x => x.CategoryId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Kategori alanı boş geçilemez.");

            RuleFor(x => x.SubcategoryId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Alt kategori alanı boş geçilemez.");

        }
    }
}
