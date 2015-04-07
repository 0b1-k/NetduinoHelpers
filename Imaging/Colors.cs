using System;
using Microsoft.SPOT;

namespace netduino.helpers.Imaging {
    // Based on MicroBuilder's code: http://www.microbuilder.eu/Projects/LPC1343ReferenceDesign/TFTLCDAPI.aspx
    
    // Basic Color definitions
    public enum BasicColor {
        Black = 0x0000,
        Blue = 0x001F,
        Red = 0xF800,
        Green = 0x07E0,
        Cyan = 0x07FF,
        Magenta = 0xF81F,
        Yellow = 0xFFE0,
        White = 0xFFFF
    }

    // Grayscale Values
    public enum GrayScaleValues {
        Gray_15 = 0x0861,    //  15  15  15
        Gray_30 = 0x18E3,    //  30  30  30
        Gray_50 = 0x3186,    //  50  50  50
        Gray_80 = 0x528A,    //  80  80  80
        Gray_128 = 0x8410,    // 128 128 128
        Gray_200 = 0xCE59,    // 200 200 200
        Gray_225 = 0xE71C     // 225 225 225
    }

    // Color Palettes
    public enum ColorTheme {
        LimeGreen_Base          = 0xD7F0,    // 211 255 130
        LimeGreen_Darker        = 0x8DE8,    // 137 188  69
        LimeGreen_Lighter       = 0xEFF9,    // 238 255 207
        LimeGreen_Shadow        = 0x73EC,    // 119 127 103
        LimeGreen_Accent        = 0xAE6D,    // 169 204 104

        Violet_Base             = 0x8AEF,    // 143  94 124
        Violet_Darker           = 0x4187,    //  66  49  59
        Violet_Lighter          = 0xC475,    // 194 142 174
        Violet_Shadow           = 0x40E6,    //  66  29  52
        Violet_Accent           = 0xC992,    // 204  50 144

        Earthy_Base             = 0x6269,    //  97  79  73
        Earthy_Darker           = 0x3103,    //  48  35  31
        Earthy_Lighter          = 0x8C30,    // 140 135 129
        Earthy_Shadow           = 0xAB29,    // 173 102  79
        Earthy_Accent           = 0xFE77,    // 250 204 188

        SkyBlue_Base            = 0x95BF,    // 150 180 255
        SkyBlue_Darker          = 0x73B0,    // 113 118 131
        SkyBlue_Lighter         = 0xE75F,    // 227 235 255
        SkyBlue_Shadow          = 0x4ACF,    //  75  90 127
        SkyBlue_Accent          = 0xB5F9     // 182 188 204
    }

    // Default Theme
    public enum DefaultColorTheme {
            Base    = ColorTheme.LimeGreen_Base,
            Darker  = ColorTheme.LimeGreen_Darker,
            Lighter = ColorTheme.LimeGreen_Lighter,
            Shadow  = ColorTheme.LimeGreen_Shadow,
            Accent = ColorTheme.LimeGreen_Accent
    }
}
