//    nVLC
//    
//    Author:  Roman Ginzburg
//
//    nVLC is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    nVLC is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.
//     
// ========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;

namespace Declarations
{
    /// <summary>
    /// Specifies the parameters of the bitmap.
    /// </summary>
    public class BitmapFormat
    {
        ChromaType m_chroma;
        int[] m_sizes = new int[3];
        
        /// <summary>
        /// Initializes new instance of BitmapFormat class
        /// </summary>
        /// <param name="width">The width of the bitmap in pixels</param>
        /// <param name="height">The height of the bitmap in pixels</param>
        /// <param name="chroma">Chroma type of the bitmap</param>
        public BitmapFormat(int width, int height, ChromaType chroma)
        {
            Width = width;
            Height = height;
            m_chroma = chroma;
            Planes = 1;
            
            Init();

            Chroma = m_chroma.ToString();
            if (IsRGB || !IsPlanarFormat)
            {
                m_sizes[0] = Pitch = Width * BitsPerPixel / 8;
                ImageSize = Pitch * Height;
            }
        }

        private void Init()
        {
            switch (m_chroma)
            {
                case ChromaType.RV15:
                    PixelFormat = PixelFormat.Format16bppRgb555;
                    BitsPerPixel = 16;
                    break;

                case ChromaType.RV16:
                    PixelFormat = PixelFormat.Format16bppRgb565;
                    BitsPerPixel = 16;
                    break;

                case ChromaType.RV24:
                    PixelFormat = PixelFormat.Format24bppRgb;
                    BitsPerPixel = 24;
                    break;

                case ChromaType.RV32:
                    PixelFormat = PixelFormat.Format32bppRgb;
                    BitsPerPixel = 32;
                    break;

                case ChromaType.RGBA:
                    PixelFormat = PixelFormat.Format32bppArgb;
                    BitsPerPixel = 32;
                    break;

                case ChromaType.NV12:
                    BitsPerPixel = 12;
                    Planes = 2;
                    m_sizes[0] = Width * Height;
                    m_sizes[1] = Width * Height / 2;
                    Pitches = new int[2] { Width, Width};
                    Lines = new int[2] { Height, Height / 2};
                    break;

                case ChromaType.I420:
                case ChromaType.YV12:
                    BitsPerPixel = 12;
                    Planes = 3;
                    m_sizes[0] = Width * Height;
                    m_sizes[1] = m_sizes[2] = Width * Height / 4;
                    Pitches = new int[3] { Width, Width / 2, Width / 2 };
                    Lines = new int[3] { Height, Height / 2, Height / 2 };
                    break;

                case ChromaType.YUY2:
                case ChromaType.UYVY:
                    BitsPerPixel = 16;
                    break;
            }
        }

        /// <summary>
        /// Gets the size in bytes of the scan line 
        /// </summary>
        public int Pitch { get; private set; }

        /// <summary>
        /// Gets the size of the image
        /// </summary>
        public int ImageSize { get; private set; }

        /// <summary>
        /// Gets the chroma type string
        /// </summary>
        public string Chroma { get; private set; }

        /// <summary>
        /// Gets the pixel format of the bitmap
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
       
        /// <summary>
        /// Gets the width of the bitmap
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the bitmap
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets number of bits used for a pixel according to ChromaType
        /// </summary>
        public int BitsPerPixel { get; private set; }

        /// <summary>
        /// Gets value indicationg whether the format contains more than one pixel plane
        /// </summary>
        public bool IsPlanarFormat
        {
            get
            {
                return m_chroma == ChromaType.I420 ||
                       m_chroma == ChromaType.NV12 ||
                       m_chroma == ChromaType.YV12;
            }
        }

        /// <summary>
        /// Gets value indicating whether the format is packed RGB
        /// </summary>
        public bool IsRGB
        {
            get
            {
                return m_chroma == ChromaType.RV15 ||
                       m_chroma == ChromaType.RV16 ||
                       m_chroma == ChromaType.RV24 ||
                       m_chroma == ChromaType.RV32 ||
                       m_chroma == ChromaType.RGBA;
            }
        }

        /// <summary>
        /// Gets number of pixel planes
        /// </summary>
        public int Planes { get; private set; }

        /// <summary>
        /// Gets array of pixel plane's sizes
        /// </summary>
        public int[] PlaneSizes
        {
            get
            {
                return m_sizes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int[] Pitches { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int[] Lines { get; private set; }
    }
}
