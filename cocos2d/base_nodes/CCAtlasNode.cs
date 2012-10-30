
using System.Diagnostics;

namespace cocos2d
{
    public class CCAtlasNode : CCNode, ICCRGBAProtocol, ICCTextureProtocol
    {
        protected bool m_bIsOpacityModifyRGB;

        protected byte m_cOpacity;
        protected CCTextureAtlas m_pTextureAtlas;
        protected ccBlendFunc m_tBlendFunc;

        protected ccColor3B m_tColor;
        protected ccColor3B m_tColorUnmodified;
        protected int m_uItemHeight;
        protected int m_uItemWidth;
        protected int m_uItemsPerColumn;
        protected int m_uItemsPerRow;

        // quads to draw
        protected int m_uQuadsToDraw;

        public CCTextureAtlas TextureAtlas
        {
            get { return m_pTextureAtlas; }
            set { m_pTextureAtlas = value; }
        }

        public int QuadsToDraw
        {
            get { return m_uQuadsToDraw; }
            set { m_uQuadsToDraw = value; }
        }

        #region ICCRGBAProtocol Members

        public bool IsOpacityModifyRGB
        {
            get { return m_bIsOpacityModifyRGB; }
            set
            {
                ccColor3B oldColor = m_tColor;
                m_bIsOpacityModifyRGB = value;
                m_tColor = oldColor;
            }
        }

        public byte Opacity
        {
            get { return m_cOpacity; }
            set
            {
                m_cOpacity = value;

                // special opacity for premultiplied textures
                if (m_bIsOpacityModifyRGB)
                {
                    Color = m_tColorUnmodified;
                }
            }
        }

        public ccColor3B Color
        {
            get
            {
                if (m_bIsOpacityModifyRGB)
                {
                    return m_tColorUnmodified;
                }
                return m_tColor;
            }
            set
            {
                m_tColor = new ccColor3B(value.r, value.g, value.b);
                m_tColorUnmodified = m_tColor;

                if (m_bIsOpacityModifyRGB)
                {
                    m_tColor.r = (byte) (value.r * m_cOpacity / 255);
                    m_tColor.g = (byte) (value.g * m_cOpacity / 255);
                    m_tColor.b = (byte) (value.b * m_cOpacity / 255);
                }

                UpdateAtlasValues();
            }
        }

        #endregion

        #region ICCTextureProtocol Members

        public ccBlendFunc BlendFunc
        {
            get { return m_tBlendFunc; }
            set { m_tBlendFunc = value; }
        }

        public virtual CCTexture2D Texture
        {
            get { return m_pTextureAtlas.Texture; }
            set
            {
                m_pTextureAtlas.Texture = value;
                UpdateBlendFunc();
                UpdateOpacityModifyRgb();
            }
        }

        #endregion

        public static CCAtlasNode Create(string tile, int tileWidth, int tileHeight, int itemsToRender)
        {
            var pRet = new CCAtlasNode();
            pRet.InitWithTileFile(tile, tileWidth, tileHeight, itemsToRender);
            return pRet;
        }

        public bool InitWithTileFile(string tile, int tileWidth, int tileHeight, int itemsToRender)
        {
            Debug.Assert(tile != null, "title should not be null");

            m_uItemWidth = tileWidth;
            m_uItemHeight = tileHeight;

            m_cOpacity = 255;
            m_tColor = m_tColorUnmodified = ccTypes.ccWHITE;
            m_bIsOpacityModifyRGB = true;

            m_tBlendFunc.src = ccMacros.CC_BLEND_SRC;
            m_tBlendFunc.dst = ccMacros.CC_BLEND_DST;

            var pNewAtlas = new CCTextureAtlas();
            pNewAtlas.InitWithFile(tile, itemsToRender);

            TextureAtlas = pNewAtlas;

            UpdateBlendFunc();
            UpdateOpacityModifyRgb();

            CalculateMaxItems();

            m_uQuadsToDraw = itemsToRender;

            return true;
        }

        private void CalculateMaxItems()
        {
            CCSize s = m_pTextureAtlas.Texture.ContentSize;
            m_uItemsPerColumn = (int) (s.height / m_uItemHeight);
            m_uItemsPerRow = (int) (s.width / m_uItemWidth);
        }

        public virtual void UpdateAtlasValues()
        {
            Debug.Assert(false, "CCAtlasNode:Abstract updateAtlasValue not overridden");
        }

        public override void Draw()
        {
            DrawManager.BlendFunc(m_tBlendFunc);

            m_pTextureAtlas.DrawNumberOfQuads(m_uQuadsToDraw, 0);
        }

        private void UpdateBlendFunc()
        {
            if (!m_pTextureAtlas.Texture.HasPremultipliedAlpha)
            {
                m_tBlendFunc.src = OGLES.GL_SRC_ALPHA;
                m_tBlendFunc.dst = OGLES.GL_ONE_MINUS_SRC_ALPHA;
            }
        }

        private void UpdateOpacityModifyRgb()
        {
            m_bIsOpacityModifyRGB = m_pTextureAtlas.Texture.HasPremultipliedAlpha;
        }
    }
}