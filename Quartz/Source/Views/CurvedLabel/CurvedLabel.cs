/*Copyright 2022 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quartz.Views
{
    public class CurvedLabel : XUiV_Label
    {
        private const string TAG = "CurvedLabel";

        private float radius = 50.0f;
        private float arcDegrees = 90.0f;
        private float angularOffsetXml = 0;
        private float angularOffset = 0;
        private int maxDegreesPerLetter = 360;

        private bool flipText = false;
        private bool drawOutwards = false;

        public CurvedLabel(string _id) : base(_id)
        {
        }

        public override void InitView()
        {
            base.InitView();
            label.onPostFill = OnPostFillCallback;

        }

        public override void UpdateData()
        {
            base.UpdateData();
            switch (alignment)
            {
                case NGUIText.Alignment.Left:
                    angularOffset = angularOffsetXml + arcDegrees / 2;
                    break;
                case NGUIText.Alignment.Right:
                    angularOffset = angularOffsetXml - arcDegrees / 2;
                    break;
                default:
                    angularOffset = angularOffsetXml;
                    break;
            }

            if (flipText)
            {
                angularOffset += 180;
            }
        }

        public override bool ParseAttribute(string attribute, string value, XUiController _parent)
        {
            float tempFloat = 0;
            if (attribute != null)
            {
                switch (attribute)
                {
                    case "radius":
                        tempFloat = radius;
                        float.TryParse(value, out radius);
                        isDirty |= tempFloat != radius;
                        return true;
                    case "arc_degrees":
                        tempFloat = arcDegrees;
                        float.TryParse(value, out arcDegrees);
                        isDirty |= tempFloat != arcDegrees;
                        return true;
                    case "angular_offset":
                        tempFloat = angularOffsetXml;
                        float.TryParse(value, out angularOffsetXml);
                        isDirty |= tempFloat != angularOffsetXml;
                        return true;
                    case "max_degrees_per_letter":
                        int tempInt = maxDegreesPerLetter;
                        int.TryParse(value, out maxDegreesPerLetter);
                        isDirty |= tempInt != maxDegreesPerLetter;
                        return true;
                    case "flip":
                        bool b = flipText;
                        flipText = StringParsers.ParseBool(value, 0, -1, true);
                        isDirty |= b != flipText;
                        return true;
                    case "draw_outwards":
                        bool outwards = flipText;
                        drawOutwards = StringParsers.ParseBool(value, 0, -1, true);
                        isDirty |= outwards != drawOutwards;
                        return true;
                    default:
                        return base.ParseAttribute(attribute, value, _parent);
                }
            }
            return false;
        }

        private void OnPostFillCallback(UIWidget widget, int bufferOffset, List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
        {
            Vector3[] boundCorners = widget.localCorners;
            float boundsMinX = boundCorners[1].x;

            int characterOffset = verts.Count / label.quadsPerCharacter;

            int textLength = characterOffset / 4;

            float lineHeight = label.finalFontSize + label.effectiveSpacingY;
            float lineY = verts[0].y;

            int line = flipText ^ drawOutwards ? ComputeMaxLineCount(verts, textLength) - 1 : 0;

            for (int i = 0; i < textLength; i++)
            {
                float charBaselineX = (verts[i * 4].x + verts[i * 4 + 2].x) / 2;
                float charBaselineY = (verts[i * 4].y + verts[i * 4 + 2].y) / 2;
                Vector3 charMidBaselinePos = new Vector2(charBaselineX, charBaselineY);

                float zeroToOnePosX = (charMidBaselinePos.x - boundsMinX) / label.width;

                if (flipText)
                {
                    zeroToOnePosX = 1 - zeroToOnePosX;
                }

                float lineDiff = Math.Abs(lineY - verts[i * 4].y);
                if (lineDiff > lineHeight / 2)
                {
                    lineY = verts[i * 4].y;

                    if (flipText ^ drawOutwards)
                    {
                        line--;
                    }
                    else
                    {
                        line++;
                    }
                }

                Matrix4x4 curveMatrix = ComputeCurveMatrix(zeroToOnePosX, lineHeight, line);

                for (int j = 0; j < label.quadsPerCharacter; j++)
                {
                    int vertIndex = j * characterOffset + i * 4;

                    verts[vertIndex] -= charMidBaselinePos;
                    verts[vertIndex + 1] -= charMidBaselinePos;
                    verts[vertIndex + 2] -= charMidBaselinePos;
                    verts[vertIndex + 3] -= charMidBaselinePos;

                    verts[vertIndex] = curveMatrix.MultiplyPoint(verts[vertIndex]);
                    verts[vertIndex + 1] = curveMatrix.MultiplyPoint(verts[vertIndex + 1]);
                    verts[vertIndex + 2] = curveMatrix.MultiplyPoint(verts[vertIndex + 2]);
                    verts[vertIndex + 3] = curveMatrix.MultiplyPoint(verts[vertIndex + 3]);
                }
            }
        }

        private Matrix4x4 ComputeCurveMatrix(float zeroToOnePosX, float lineHeight, int line)
        {
            float actualArcDegrees = Mathf.Min(arcDegrees, label.text.Length * maxDegreesPerLetter);

            float angle = ((zeroToOnePosX - 0.5f) * actualArcDegrees + angularOffset) * Mathf.Deg2Rad;

            float x0 = Mathf.Cos(angle);
            float y0 = Mathf.Sin(angle);

            float newRadius;
            if (!(flipText || drawOutwards) || flipText && !drawOutwards)
            {
                newRadius = radius - lineHeight * line;
                Logging.Out(TAG, "radius - line height");
            }
            else
            {
                newRadius = radius + lineHeight * line;
                Logging.Out(TAG, "radius + line height");
            }

            Vector2 newMideBaselinePos = new Vector2(x0 * newRadius, -y0 * newRadius);

            Quaternion rotation = Quaternion.AngleAxis(-Mathf.Atan2(y0, x0) * Mathf.Rad2Deg - 90, Vector3.forward);
            if (flipText)
            {
                rotation *= Quaternion.AngleAxis(180, Vector3.forward);
            }

            return Matrix4x4.TRS(new Vector3(newMideBaselinePos.x, newMideBaselinePos.y, 0), rotation, Vector3.one);
        }

        private int ComputeMaxLineCount(List<Vector3> verts, int textLength)
        {
            int line = 0;
            float lineHeight = label.finalFontSize + label.effectiveSpacingY;
            float lineY = verts[0].y;

            for (int i = 0; i < textLength; i++)
            {
                float lineDiff = Math.Abs(lineY - verts[i * 4].y);
                if (lineDiff > lineHeight / 2)
                {
                    lineY = verts[i * 4].y;
                    line++;
                }
            }

            return line + 1;
        }
    }
}