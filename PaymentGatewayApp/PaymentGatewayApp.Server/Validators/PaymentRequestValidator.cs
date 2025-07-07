using FluentValidation;
using PaymentGatewayApp.Server.Requests;

namespace PaymentGatewayApp.Server.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequests>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.");

            RuleFor(x => x.PaymentMode)
                .NotEmpty().WithMessage("Payment mode is required.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Account number is required.")
                .When(x => x.PaymentMode == "Bank");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.")
                .When(x => x.PaymentMode == "Bank");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Card number is required.")
                .When(x => x.PaymentMode == "Card");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Expiry date is required.")
                .When(x => x.PaymentMode == "Card");

            RuleFor(x => x.CVV)
                .NotEmpty().WithMessage("CVV is required.")
                .When(x => x.PaymentMode == "Card");
        }

    }
}
