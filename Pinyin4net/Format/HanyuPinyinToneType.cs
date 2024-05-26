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
    /// Define the output format of Hanyu Pinyin tones.
    /// 
    /// Chinese has four pitched tones and a "toneless" tone. They are called Píng(平,
    /// flat), Shǎng(上, rise), Qù(去, high drop), Rù(入, drop) and Qing(轻, toneless).
    /// Usually, we use 1, 2, 3, 4 and 5 to represent them.
    /// 
    /// This class provides several options for output of Chinese tones, which are
    /// listed below. For example, Chinese character '打'
    /// 
    /// 1、WITH_TONE_NUMBER  -> da3
    /// 2、WITHOUT_TONE      -> da
    /// 3、WITH_TONE_MARK    -> dǎ
    /// </summary>
    public enum HanyuPinyinToneType
    {
        WITH_TONE_NUMBER,   //  With tone numbers, for example: li3.
        WITHOUT_TONE,       //  Without tone numbers.
        WITH_TONE_MARK      //  With tone marks
    }
}
