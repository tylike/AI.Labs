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
    /// Define the output format of character 'ü';
    /// 
    /// 'ü' is a special character of Hanyu pinyin, which can not be simply
    /// represented by English letters. In Hanyu pinyin, such characters include 'ü',
    /// 'üe', 'üan', and 'ün'.
    /// 
    /// This class provides several options for output of 'ü', which are listed
    /// below:
    /// 
    /// 1. WITH_U_AND_COLON -> u:
    /// 2. WITH_V           -> v
    /// 3. WITH_U_UNICODE   -> ü
    /// </summary>
    public enum HanyuPinyinVCharType
    {
        WITH_U_AND_COLON,       //  This option indicates that the output of 'ü' is "u:"
        WITH_V,                 //  This option indicates that the output of 'ü' is "v"
        WITH_U_UNICODE          //  This option indicates that the output of 'ü' is "ü" in Unicode form
    }
}
