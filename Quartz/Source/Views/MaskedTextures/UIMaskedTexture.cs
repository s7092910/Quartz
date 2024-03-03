/*Copyright 2023 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System.Collections.Generic;
using UnityEngine;

namespace Quartz.Views
{
    public class UIMaskedTexture : UIBasicSprite
    {

        private Rect mRect = new Rect(0f, 0f, 1f, 1f);
        private Texture mTexture;
        private Shader mShader;
        private Vector4 mBorder = Vector4.zero;
        private bool mFixedAspect = false;

        //TODO: Test Unlit/Transparent Masked instead
        private Texture mMaskTexture;
        private bool mRebuildMaterial = true;
        private bool mFixMaskUV = true;

        public override Texture mainTexture
        {
            get
            {
                return mTexture;
            }
            set
            {
                if (mTexture != value)
                {
                    RemoveFromPanel();

                    mTexture = value;
                    UpdateMaterial();

                    MarkAsChanged();
                }
            }
        }

        public Texture maskTexture
        {
            get { return mMaskTexture; }
            set
            {
                if (mMaskTexture != value)
                {
                    RemoveFromPanel();

                    if (mRebuildMaterial)
                        RemoveMaterial();
                    mMaskTexture = value;
                    UpdateMaterial();

                    MarkAsChanged();
                }
            }
        }

        public bool isRebuildMaterial
        {
            get { return mRebuildMaterial; }
            set { mRebuildMaterial = value; }
        }

        public override Material material
        {
            get
            {
                UpdateMaterial();
                return mMat;
            }
            set
            {
                RemoveFromPanel();

                mMat = value;
                UpdateMaterial();

                MarkAsChanged();
            }
        }

        public override Shader shader
        {
            get
            {
                if (mShader == null)
                {
                    mShader = LoadShader();
                    UpdateMaterial();
                }
                return mShader;
            }
            set
            {
                if (mShader != value)
                {
                    RemoveFromPanel();

                    if (mRebuildMaterial)
                        RemoveMaterial();
                    mShader = value;
                    UpdateMaterial();

                    MarkAsChanged();
                }
            }
        }

        public override bool premultipliedAlpha
        {
            get { return false; }
        }


        public override Vector4 border
        {
            get
            {
                return mBorder;
            }
            set
            {
                if (mBorder != value)
                {
                    mBorder = value;
                    MarkAsChanged();
                    UpdateMaterial();
                }
            }
        }

        public Rect uvRect
        {
            get
            {
                return mRect;
            }
            set
            {
                if (mRect != value)
                {
                    mRect = value;
                    MarkAsChanged();
                    UpdateMaterial();
                }
            }
        }

        public bool fixMaskUV
        {
            get { return mFixMaskUV; }
            set
            {
                mFixMaskUV = value;
                UpdateMaterial();
            }
        }

        public override Vector4 drawingDimensions
        {
            get
            {
                Vector2 offset = pivotOffset;

                float x0 = -offset.x * mWidth;
                float y0 = -offset.y * mHeight;
                float x1 = x0 + mWidth;
                float y1 = y0 + mHeight;

                if (mTexture != null && mType != UISprite.Type.Tiled)
                {
                    int w = mTexture.width;
                    int h = mTexture.height;
                    int padRight = 0;
                    int padTop = 0;

                    float px = 1f;
                    float py = 1f;

                    if (w > 0 && h > 0 && (mType == UISprite.Type.Simple || mType == UISprite.Type.Filled))
                    {
                        if ((w & 1) != 0) ++padRight;
                        if ((h & 1) != 0) ++padTop;

                        px = (1f / w) * mWidth;
                        py = (1f / h) * mHeight;
                    }

                    if (mFlip == UISprite.Flip.Horizontally || mFlip == UISprite.Flip.Both)
                        x0 += padRight * px;
                    else
                        x1 -= padRight * px;

                    if (mFlip == UISprite.Flip.Vertically || mFlip == UISprite.Flip.Both)
                        y0 += padTop * py;
                    else
                        y1 -= padTop * py;
                }

                float fw, fh;

                if (mFixedAspect)
                {
                    fw = 0f;
                    fh = 0f;
                }
                else
                {
                    Vector4 br = border;
                    fw = br.x + br.z;
                    fh = br.y + br.w;
                }

                float vx = Mathf.Lerp(x0, x1 - fw, mDrawRegion.x);
                float vy = Mathf.Lerp(y0, y1 - fh, mDrawRegion.y);
                float vz = Mathf.Lerp(x0 + fw, x1, mDrawRegion.z);
                float vw = Mathf.Lerp(y0 + fh, y1, mDrawRegion.w);

                return new Vector4(vx, vy, vz, vw);
            }
        }

        public bool fixedAspect
        {
            get
            {
                return mFixedAspect;
            }
            set
            {
                if (mFixedAspect != value)
                {
                    mFixedAspect = value;
                    mDrawRegion = new Vector4(0f, 0f, 1f, 1f);
                    MarkAsChanged();
                    UpdateMaterial();
                }
            }
        }

        public void FitTextureInMask()
        {
            var texture = mainTexture;
            if (texture == null)
                return;

            float textureRatio = (float)texture.width / texture.height;
            float widgetRatio = (float)width / height;

            // If the texture is vertically longer in terms of widget ratio
            if (textureRatio > widgetRatio)
            {
                // Fit texture to height.
                float sizeX = height / textureRatio / width;
                float offsetX = (1f - sizeX) / 2f;
                uvRect = new Rect(offsetX, 0f, sizeX, 1f);
            }
            else if (textureRatio < widgetRatio)
            {
                // Fit texture to width.
                float sizeY = height / (width / textureRatio);
                float offsetY = (1f - sizeY) / 2f;
                uvRect = new Rect(0f, offsetY, 1f, sizeY);
            }
        }

        public override void MakePixelPerfect()
        {
            base.MakePixelPerfect();
            if (mType == Type.Tiled)
                return;

            Texture tex = mainTexture;
            if (tex == null)
                return;

            if (mType == Type.Simple || mType == Type.Filled || !hasBorder)
            {
                if (tex != null)
                {
                    int w = tex.width;
                    int h = tex.height;

                    if ((w & 1) == 1)
                        ++w;
                    if ((h & 1) == 1)
                        ++h;

                    width = w;
                    height = h;
                }
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (mFixedAspect)
            {
                Texture tex = mainTexture;

                if (tex != null)
                {
                    int w = tex.width;
                    int h = tex.height;
                    if ((w & 1) == 1)
                        ++w;
                    if ((h & 1) == 1)
                        ++h;

                    float widgetWidth = mWidth;
                    float widgetHeight = mHeight;
                    float widgetAspect = widgetWidth / widgetHeight;
                    float textureAspect = (float)w / h;

                    if (textureAspect < widgetAspect)
                    {
                        float x = (widgetWidth - widgetHeight * textureAspect) / widgetWidth * 0.5f;
                        drawRegion = new Vector4(x, 0f, 1f - x, 1f);
                    }
                    else
                    {
                        float y = (widgetHeight - widgetWidth / textureAspect) / widgetHeight * 0.5f;
                        drawRegion = new Vector4(0f, y, 1f, 1f - y);
                    }
                }
            }
        }

        public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
        {
            Texture tex = mainTexture;
            if (tex == null)
                return;

            Rect outer = new Rect(mRect.x * tex.width, mRect.y * tex.height, tex.width * mRect.width, tex.height * mRect.height);
            Rect inner = outer;
            Vector4 br = border;
            inner.xMin += br.x;
            inner.yMin += br.y;
            inner.xMax -= br.z;
            inner.yMax -= br.w;

            float w = 1f / tex.width;
            float h = 1f / tex.height;

            outer.xMin *= w;
            outer.xMax *= w;
            outer.yMin *= h;
            outer.yMax *= h;

            inner.xMin *= w;
            inner.xMax *= w;
            inner.yMin *= h;
            inner.yMax *= h;

            int offset = verts.Count;
            Fill(verts, uvs, cols, outer, inner);

            onPostFill?.Invoke(this, offset, verts, uvs, cols);
        }

        private void ResetMaterial()
        {
            RemoveMaterial();
            UpdateMaterial();
        }

        private void RemoveMaterial()
        {
            if (mMat != null)
            {
                Destroy(mMat);
                mMat = null;
            }
        }

        void CreateMaterial()
        {
            if(mShader == null)
            {
                mShader = LoadShader();
            }
            if (mMat == null)
            {
                mMat = new Material(mShader);
            }
        }

        void UpdateMaterial()
        {
            CreateMaterial();

            mMat.SetTexture("_Mask", mMaskTexture);

            Vector2 offset;
            Vector2 scale;
            if (mFixMaskUV)
            {
                offset = new Vector2(mRect.x, mRect.y);
                scale = new Vector2(mRect.width, mRect.height);
            }
            else
            {
                offset = Vector2.zero;
                scale = Vector2.one;
            }

            mMat.SetTextureOffset("_MainTex", offset);
            mMat.SetTextureScale("_MainTex", scale);

            if (drawCall != null && drawCall.dynamicMaterial != null)
            {
                drawCall.dynamicMaterial.SetTextureOffset("_MainTex", new Vector2(mRect.x, mRect.y));
                drawCall.dynamicMaterial.SetTextureScale("_MainTex", new Vector2(mRect.width, mRect.height));
            }
        }

        private Shader LoadShader()
        {
            return DataLoader.LoadAsset<Shader>("#@modfolder(Quartz)://Resources/quartzshaders.unity3d?Assets/MaskedTexture/UnlitTransparentFixableMask.shader");
        }
    }

}
