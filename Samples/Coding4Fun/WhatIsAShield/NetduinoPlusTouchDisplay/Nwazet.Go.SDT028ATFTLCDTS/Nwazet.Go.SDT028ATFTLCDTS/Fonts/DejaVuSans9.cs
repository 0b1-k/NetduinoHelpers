namespace Nwazet.Go.Fonts {
    // Font data for DejaVu Sans 9pt
    public class DejaVuSans9 : FontDefinition {
        public static readonly ushort ID = 30367;
        public override FontInfo GetFontInfo() {
            return new FontInfo(
                height: 13, // Character height
                startChar: ' ', // Start character
                endChar: '~', // End character
                fontDescriptors: DejaVuSans9ptDescriptors, // Character descriptor array
                fontBitmaps: null, // Character bitmap array
                name: GetFontName(),
                id: ID
                );
        }
        // Character descriptors for DejaVu Sans 9pt */
        // { [Char width in bits], [Offset into dejaVuSans9ptCharBitmaps in bytes] }
        public ushort[] DejaVuSans9ptDescriptors = {
	        5, 0, 		    /*   */ 
	        1, 13, 		    /* ! */ 
	        3, 26, 		    /* " */ 
	        8, 39, 		    /* # */ 
	        5, 52, 		    /* $ */ 
	        10, 65, 		/* % */ 
	        8, 91, 		    /* & */ 
	        1, 104, 		/* ' */ 
	        3, 117, 		/* ( */ 
	        3, 130, 		/* ) */ 
	        5, 143, 		/* * */ 
	        7, 156, 		/* + */ 
	        1, 169, 		/* , */ 
	        3, 182, 		/* - */ 
	        1, 195, 		/* . */ 
	        4, 208, 		/* / */ 
	        6, 221, 		/* 0 */ 
	        5, 234, 		/* 1 */ 
	        6, 247, 		/* 2 */ 
	        6, 260, 		/* 3 */ 
	        6, 273, 		/* 4 */ 
	        6, 286, 		/* 5 */ 
	        6, 299, 		/* 6 */ 
	        6, 312, 		/* 7 */ 
	        6, 325, 		/* 8 */ 
	        6, 338, 		/* 9 */ 
	        1, 351, 		/* : */ 
	        1, 364, 		/* ; */ 
	        8, 377, 		/* < */ 
	        8, 390, 		/* = */ 
	        8, 403, 		/* > */ 
	        5, 416, 		/* ? */ 
	        11, 429, 		/* @ */ 
	        8, 455, 		/* A */ 
	        6, 468, 		/* B */ 
	        6, 481, 		/* C */ 
	        7, 494, 		/* D */ 
	        6, 507, 		/* E */ 
	        5, 520, 		/* F */ 
	        7, 533, 		/* G */ 
	        7, 546, 		/* H */ 
	        1, 559, 		/* I */ 
	        3, 572, 		/* J */ 
	        6, 585, 		/* K */ 
	        5, 598, 		/* L */ 
	        8, 611, 		/* M */ 
	        7, 624, 		/* N */ 
	        7, 637, 		/* O */ 
	        6, 650, 		/* P */ 
	        7, 663, 		/* Q */ 
	        7, 676, 		/* R */ 
	        6, 689, 		/* S */ 
	        7, 702, 		/* T */ 
	        7, 715, 		/* U */ 
	        8, 728, 		/* V */ 
	        11, 741, 		/* W */ 
	        7, 767, 		/* X */ 
	        7, 780, 		/* Y */ 
	        7, 793, 		/* Z */ 
	        2, 806, 		/* [ */ 
	        4, 819, 		/* \ */ 
	        2, 832, 		/* ] */ 
	        6, 845, 		/* ^ */ 
	        6, 858, 		/* _ */ 
	        2, 871, 		/* ` */ 
	        6, 884, 		/* a */ 
	        6, 897, 		/* b */ 
	        5, 910, 		/* c */ 
	        6, 923, 		/* d */ 
	        6, 936, 		/* e */ 
	        4, 949, 		/* f */ 
	        6, 962, 		/* g */ 
	        6, 975, 		/* h */ 
	        1, 988, 		/* i */ 
	        2, 1001, 		/* j */ 
	        5, 1014, 		/* k */ 
	        1, 1027, 		/* l */ 
	        9, 1040, 		/* m */ 
	        6, 1066, 		/* n */ 
	        6, 1079, 		/* o */ 
	        6, 1092, 		/* p */ 
	        6, 1105, 		/* q */ 
	        4, 1118, 		/* r */ 
	        5, 1131, 		/* s */ 
	        4, 1144, 		/* t */ 
	        6, 1157, 		/* u */ 
	        6, 1170, 		/* v */ 
	        9, 1183, 		/* w */ 
	        6, 1209, 		/* x */ 
	        6, 1222, 		/* y */ 
	        5, 1235, 		/* z */ 
	        5, 1248, 		/* { */ 
	        1, 1261, 		/* | */ 
	        5, 1274, 		/* } */ 
	        8, 1287 		/* ~ */ 
        };
    }
}
