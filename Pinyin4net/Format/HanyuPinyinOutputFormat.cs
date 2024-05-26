/**
 * Copyright (c) 2012 Yang Kuang
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pinyin4net.Format
{
    /// <summary>
    /// This classes define how the Hanyu Pinyin should be outputted.
    /// 
    /// The output feature includes:
    ///     Output format of character 'ü';
    ///     Output format of Chinese tones;
    ///     Cases of letters in outputted string.
    ///     
    /// Default value of these features are list below:
    ///     HanyuPinyinVCharType: HanyuPinyinVCharType.WITH_V;
    ///     HanyuPinyinCaseType: HanyuPinyinCaseType.LOWERCASE;
    ///     HanyuPinyinToneType: HanyuPinyinToneType.WITH_TONE_NUMBER;
    /// </summary>
    public class HanyuPinyinOutputFormat
    {
        private HanyuPinyinVCharType _vcharType = HanyuPinyinVCharType.WITH_V;
        private HanyuPinyinCaseType _caseType = HanyuPinyinCaseType.LOWERCASE;
        private HanyuPinyinToneType _toneType = HanyuPinyinToneType.WITH_TONE_NUMBER;
        
        public HanyuPinyinVCharType VCharType
        {
            get { return _vcharType; }
            set { _vcharType = value; }
        }

        public HanyuPinyinCaseType CaseType
        {
            get { return _caseType; }
            set { _caseType = value; }
        }

        public HanyuPinyinToneType ToneType
        {
            get { return _toneType; }
            set { _toneType = value; }
        }

        public HanyuPinyinOutputFormat() { }

        
    }
}
