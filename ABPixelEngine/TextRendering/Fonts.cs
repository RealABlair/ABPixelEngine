using System;

namespace ABSoftware.ABPixelEngine.TextRendering
{
    public class Fonts
    {
        public static string GetFontCode(Font font)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            int width = font.AtlasWidth;
            int height = font.AtlasHeight;

            uint r = 0;
            int bitCounter = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte pixel = font.FontMask[y * width + x];

                    if(pixel == 255)
                    {
                        r |= (1u << bitCounter);
                    }

                    bitCounter++;

                    if(bitCounter == 24)
                    {
                        uint c1 = (r >> 18) & 0x3F;
                        uint c2 = (r >> 12) & 0x3F;
                        uint c3 = (r >> 6)  & 0x3F;
                        uint c4 = r         & 0x3F;

                        builder.Append((char)(c1 + 48));
                        builder.Append((char)(c2 + 48));
                        builder.Append((char)(c3 + 48));
                        builder.Append((char)(c4 + 48));

                        r = 0;
                        bitCounter = 0;
                    }
                }
            }

            if(bitCounter > 0)
            {
                uint c1 = (r >> 18) & 0x3F;
                uint c2 = (r >> 12) & 0x3F;
                uint c3 = (r >> 6) & 0x3F;
                uint c4 = r & 0x3F;

                builder.Append((char)(c1 + 48));
                builder.Append((char)(c2 + 48));
                builder.Append((char)(c3 + 48));
                builder.Append((char)(c4 + 48));
            }

            return builder.ToString();
        }

        public static Font GetFontFromCode(int characterWidth, int characterHeight, int rows, int columns, string fontCode)
        {
            Font font = new Font(characterWidth, characterHeight, rows, columns);
            byte[] fontMask = font.FontMask;
            
            int width = font.AtlasWidth;
            int height = font.AtlasHeight;
            int px = 0;
            int py = 0;

            for(int i = 0; i < fontCode.Length; i += 4)
            {
                if (i + 3 >= fontCode.Length)
                    break;

                uint c1 = (uint)fontCode[i + 0] - 48;
                uint c2 = (uint)fontCode[i + 1] - 48;
                uint c3 = (uint)fontCode[i + 2] - 48;
                uint c4 = (uint)fontCode[i + 3] - 48;
                uint r = c1 << 18 | c2 << 12 | c3 << 6 | c4;

                for(int j = 0; j < 24; j++)
                {
                    if (py >= height)
                        return font;

                    byte k = (byte)((r & (1u << j)) != 0 ? 255 : 0);

                    fontMask[py * width + px] = k;

                    px++;
                    if(px == width)
                    {
                        px = 0;
                        py++;
                    }
                }
            }

            return font;
        }

        #region CP437_8x8
        public static Font CP437_8X8 = GetFontFromCode(8, 8, 16, 16,
            @"OWh070Pfo`00l?l0o_`lP@2I77oo01Pn`ccoc6KP05[6Om^U?1`nI^LHI_2I??kloh40Ogmo`c" +
            @"an_[e2aPaV^@3W?Sk7??mo_D;333`c0>O67>>MOPPLI^LH63>I?>H>oh4020P8o`0H<l<lI`mn" +
            @"OP1J001n07hno`3o1aPN0IT3IQQ00?cn61PH00000000?7071]]V?3`031PH62@0O1ooffInOP" +
            @"0n<1QnIP<6OollIQQo06KN61PH0gmoOWko67`OI]QV67in1S1nofH3L0Llf01n?7il63`H97l<" +
            @"0ASoISa0OV3H61PH000000006000?`000?l000000000600063Hf31`000``00000600=SHH=V" +
            @"=n61P<01QV<000=QP0<`=o30HL63``0000600H?SH006hHoc0<OP1n00`0O`0H>`aP<0`001Pl" +
            @"1P000000ISlf600c66HH600H600363H006iS00``01P0004H00000000000030000000?QPnOc" +
            @"0o?Wln000n300`77<l<f1SH0<366=S01PHNfHHH60H0clcHf=P31PH<31n?QQ_H3<l?S0o001n" +
            @"H00666LHOf0366=P061SOP`0HaP`H0<HHf<`H6<<61PH01P0Oghn?S0o?P`n61Pn300`000H00" +
            @"0000000000000<?P00?SlLOglOOV<n0f=P?V=SHcIS0c=SHf<3<f0HIgL3Hg]SH`=S0`<3H1QS" +
            @"O`<KNf=_0cmS7amS67l30`mPHg]oHgmk0f<3Hg<36f0HLf\3H`=S<f=SH`<3HaQSH`<cOV=S?S" +
            @"mS0glOOV<nOf<n?V=S00000000000000000000?Sl0ochoHf=SOf=S?0<lH`08Hf=SHf<HHf=S" +
            @"1PaP01``Hf=SHaP3=V=S331S=S0<Hcl063hoHf=S63hL<1P<0`1SH1][Hf<H63I[<0`<000`<g" +
            @"\3HaQSHgln30H<031P?P<063iS=QaoO`MS?40l0000001P000000000000o`000`0H06000`0h" +
            @"0f0H000L01P0H00306`0000301P3<000?P<n33iP73mn63=P?Slc?f00HgiSHf<N6f0HHglHOP" +
            @"1SH`=SH`aoH1QSOaPO06=SHf=S30=S66=n63=PHf=[?gh0?WhnHf0NHf<lHf\l000n0000?`00" +
            @"?P00000000000000000H0000670006h>000001P000006000>aPHOSl8OWhoHf=SOf=S61PHHa" +
            @"`00f=SHf<HHcIS00h`=P1`0f=SHaPn76]S61aS01PHOSmS6603OciS1Whf61PH0f<0?`=P77i`" +
            @"H6<f671oO`0>060300000000000o0000Hch0IWi`01P71fIn3SiVHaaSP@0060000840@@0070" +
            @"P0?V<3?Shl?Shn73hn?Q`LH`<0H61SHf1PHf=S61PH0ciSOWmSOWinOgl361QoHglH0f=[Hf=S" +
            @"0f]S60<3HaPHOSioOWhn?Win?Shn?3`l36=S00000000000<00000000o01`1fHnIPLn66=S7f" +
            @"HL001`044f044003h0ISHHOmPc?S?n03hnHfIS1WiS63<loc03Hf=SIV=S6f=SGaP?oQllHf<c" +
            @"Hf=SHf=VOPHK0aQSHc<cHf=SHgaSIQ]S6?<Hloio?ShnH7inOWlnHgho000K00000000000n60" +
            @"0HL0k3L70h?7l001Pl`f<0000H000007l006Hf<`00c01S73hc06<nISIS000H63<KH6IVHf<H" +
            @"O6Lo?aPlfo`oc3<HHaQnKf=S3000aS03IQS\66=VHf=SOWik<0=V6;KcOS?<OShl07=S03`0lc" +
            @"40000H000000000000l0000030ZT@061Rk06`HK6`0K6`04@0H6>iEK1PHK000K01\01Q\^jY4" +
            @"7aPH7`1\Ofa_7fa_EA4061S^06`HK60HK61PA00H6;^ZKalOKamoOfm\7amokUDA61PH66a\K6" +
            @"a\0000ZT@H61RkK6`HK6`H001\4AP06>iEK1PHK1Q\06a\600001PH600HK6`H06`0K01\61PH" +
            @"01P0K1PHK01\06`061Q\600Hn1P0o>a\k?o_onoo01PH600H36`H000<000<ooP0ooSok?Soon" +
            @"clonc_0?o_61P061P0K01\K6`006`06000600H06`HK01\K01\000001PHK1PH06`006a\K01\" +
            @"K000000H61Q\0?l0oo0?001\01Q\66`0o`0Hl0l0ofcon6`0K03h01So3`3oK?o`K00001PH61" +
            @"Q\0?l0oo0?oooon?SlooolooPOl0oo6000001\K6`H600H3ooo003`06`HK1P001Q\oolH0?0?" +
            @"K1P0600066a\oaP0l0oo7000O`000000?3anH01`01hL07lfIP06IQQ^01QV<`H`HcI^OPao?3" +
            @"]V<<?30gin7S\c63H366HK`omVfm]n4c<O=P<fIQ\<`fHHff=V<`?K0fHk6`Hf?1PnHfIV1Win" +
            @"=VhcOcH360H>ic`H1P0n03<L00060`0007h0000000031QP0670H700Hl00003hN67h0f0`<KQ" +
            @"PH000f<3H`00P01QQn01SH03Hk=S0003`n31Qn61P<701n<00H?0Hf600061P6KP0H61P0?SHg" +
            @"OP0l000061\H000k03H003`0?gh06aPo000H?000000000006000000>0000000h0000");
        #endregion
        #region IBM_DCA_4x7
        public static Font IBM_DCA_4X7 = GetFontFromCode(4, 7, 16, 16,
            "00003`l00000000000U_1V00VB8RGV9VMeo@9YUgT2XU9bMi<UU_9V1O3`8R9AL_00003`00050" +
            "200000000000000000000Ze80029`@P0085]GDB@W0VH0LW2ZL_ooJUM2928_02M@L102000000" +
            "00000000000U001@0280009IKe028TEB10@R93871g9_008T8=82001Y=@10DT000B000200800" +
            "0000000000000000000Lc808W9d<@@R=R@U05M39D9b9DM20@1U=b8T8R8d8W8R000040000044" +
            "0000000000H000000000Lf<YE7EWGBFA4cDClC@W5GeG9E4EEi4eLfMBDWEQ02FG00000000000" +
            "000000P00000000008`D0UEMSHFME=ED0DYEB048VDT=C8b;b8@12TW8eI6LU04000000l00000" +
            "0200001000000000@P0@000@h400<@IV<`E3MWH2=BLQDELS@UHe1EMD9V9EMD00000`0`00000" +
            "0000P00000:00001@800P008000H`0REEMg8RME8EEPHW9BT48B=W5S9dEB@O0R00008R1P9500" +
            "0P0P9A82AE80E5D21P0U00000000=U4RIV<cEC<fIV5A8A4F=WLRIEEFMgIV009E02000000420" +
            "11@0B0P009BE@001E3PL00000@eHU8RgcIBEEhBEBEEEG:QEF;EMo93<R9GMW000080H0A10P07" +
            "Y400008R8000050200D000T08S<`01EW=B;EobEBIJ4BEeYBjXEC9ef1H005XYM`0000009G@0D" +
            "0DR0UD58RZZ1E05d@9E1C8UEOEC:[X3@WDS<WB5<RED]E=EhR<7EB8[0:EB9EDPDUDR802U820R" +
            "1@YB8RD5DR0P1@D:828UnUhPoOgmonon01;B0001GR0RoMoN8P3mD58P1@E@8280D51B1@D5D09" +
            "@<?0RD0Gl8U02o33`kU3el2m@1O``ob;o?ohb1Ool3eG^33ob80DPlP9E80`oEB05?o82000<00" +
            "00000000000000<000d0Mg@68RDEhTDREREVlEDU4e9EDUFU9E9bMA:eDR031D0P40010P00000" +
            "000000000000002@Rl01B8T1bM08RA`@V9@;`M59b92H300IW07050R000P4P0?00020000");
        #endregion
        #region ANAPAN_4x4
        public static Font ANAPAN_4X4 = GetFontFromCode(4, 4, 16, 16,
            "8W8PI`L2D5MR9eMg9ELU8R0U1`MbECLW00000000H`0086MB0P0RDWMb8R0WMbHf=U9S029c02L" +
            "000000000EB008R9F@00U8P0PLT47821`0U=@0BDR000B00000000IG<g8WIa9F=fAbMR0`1gMW" +
            "M6AdLd8gHR000000009`00=g=cLEAe5gDS9e<eEG4eLgMCEgEa02EG00000000IbL0EEEG0S5SD" +
            "R5G8R9eH@DRLW8a=6HU001`00000000LS4QE04gDR02Dc<CLSDW<e1C9G5c9E9B00000000<`00" +
            "00808R1EHc<S8UEGET8CHS4Q9REb070R000000009`L0Mc8R8P07Mg=A8c<G=gLR=EEG8RMg001" +
            "E000000008R8W9BD0LgMAEEEG8AEB9ELg8VHR4BLW000000000P00<c029E02EBLfM3=CHb8WEC" +
            "8UE1H003HU00000000:ZX0D`EB0eEeDbm@AD9eZS9dEE8Z1eEC;e0SDUEB801E820RoOeNo_gmd" +
            "_o_0@0A8P00gehPoOgO8280D51Bm@D5D>i@<?0_o`ol<_lRo33nkPG`lPmE80`oEB05?o82McH<" +
            "8VEgMVMR<eEcEGDeMU=b8cMA=bDR001F00000000A6<W@0=RM`0c=dXf8e03MgL2<09B000R8P0" +
            "000000000");
        #endregion
    }
}
