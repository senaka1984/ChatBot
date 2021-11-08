using System.Text.RegularExpressions;
using Serilog.Enrichers.Sensitive;

namespace ChatBot.Common.Serilog.Masking
{
    public class NRICMaskingOperator : RegexMaskingOperator //MyRegexMaskingOperator <-- replace to debug
    {
        private const string NRICMaskingPattern = @"(?<leading3>\b[stfgpSTFGP]\d{2})(?<toMask>\d{4})(?<trailing>\w+)";
        private readonly string _replacementPattern;
        public NRICMaskingOperator() : base(NRICMaskingPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
            _replacementPattern = "${{leading3}}{0}${{trailing}}";
        }

        protected override string PreprocessMask(string mask) => string.Format(_replacementPattern, mask);
    }
}

/// Use this to debug
//     public abstract class MyRegexMaskingOperator : IMaskingOperator
//     {
//         private readonly Regex _regex;

//         protected MyRegexMaskingOperator(string regexString)
//             : this(regexString, RegexOptions.Compiled)
//         {
//         }

//         protected MyRegexMaskingOperator(string regexString, RegexOptions options)
//         {
//             _regex = new Regex(regexString ?? throw new ArgumentNullException("regexString"), options);
//             if (string.IsNullOrWhiteSpace(regexString))
//             {
//                 throw new ArgumentOutOfRangeException("regexString", "Regex pattern cannot be empty or whitespace.");
//             }
//         }

//         public MaskingResult Mask(string input, string mask)
//         {
//             string input2 = PreprocessInput(input);
//             if (!ShouldMaskInput(input2))
//             {
//                 return MaskingResult.NoMatch;
//             }
//             string text = _regex.Replace(input2, PreprocessMask(mask));
//             MaskingResult result = default(MaskingResult);
//             result.Result = text;
//             result.Match = text != input;
//             return result;
//         }

//         protected virtual bool ShouldMaskInput(string input)
//         {
//             return true;
//         }

//         protected virtual string PreprocessInput(string input)
//         {
//             return input;
//         }

//         protected virtual string PreprocessMask(string mask)
//         {
//             return mask;
//         }
//     }
