﻿using System;
using Intersect.Client.Framework.File_Management;
using Intersect.Client.Framework.GenericClasses;
using Intersect.Client.Framework.Graphics;
using Intersect.Client.General;
using Intersect.GameObjects;

namespace Intersect.Client.Entities
{
    public class AnimationInstance
    {
        private int mRenderDir;
        private float mRenderX;
        private float mRenderY;
        public bool AutoRotate;
        public bool Hidden;
        public bool InfiniteLoop;
        private int mLowerFrame;
        private int mLowerLoop;
        private long mLowerTimer;
        public AnimationBase MyBase;
        private bool mShowLower = true;
        private bool mShowUpper = true;
        private MapSound mSound;
        private int mUpperFrame;
        private int mUpperLoop;
        private long mUpperTimer;
        private int mZDimension = -1;
        private Entity mParent;

        public AnimationInstance(AnimationBase animBase, bool loopForever, bool autoRotate = false, int zDimension = -1, Entity parent = null)
        {
            MyBase = animBase;
            mParent = parent;
            if (MyBase != null)
            {
                mLowerLoop = animBase.Lower.LoopCount;
                mUpperLoop = animBase.Upper.LoopCount;
                mLowerTimer = Globals.System.GetTimeMs() + animBase.Lower.FrameSpeed;
                mUpperTimer = Globals.System.GetTimeMs() + animBase.Upper.FrameSpeed;
                InfiniteLoop = loopForever;
                AutoRotate = autoRotate;
                mZDimension = zDimension;
                mSound = GameAudio.AddMapSound(MyBase.Sound, 0, 0,Guid.Empty, loopForever, 12);
                lock (GameGraphics.AnimationLock)
                {
                    GameGraphics.LiveAnimations.Add(this);
                }
            }
            else
            {
                Dispose();
            }
        }

        public void Draw(bool upper = false, bool alternate = false)
        {
            if (Hidden) return;
            if (!upper && alternate != MyBase.Lower.AlternateRenderLayer) return;
            if (upper && alternate != MyBase.Upper.AlternateRenderLayer) return;
            var rotationDegrees = 0f;
            var dontRotate = upper && MyBase.Upper.DisableRotations || !upper && MyBase.Lower.DisableRotations;
            if ((AutoRotate || mRenderDir != -1) && !dontRotate)
            {
                switch (mRenderDir)
                {
                    case 0: //Up
                        rotationDegrees = 0f;
                        break;
                    case 1: //Down
                        rotationDegrees = 180f;
                        break;
                    case 2: //Left
                        rotationDegrees = 270f;
                        break;
                    case 3: //Right
                        rotationDegrees = 90f;
                        break;
                    case 4: //NW
                        rotationDegrees = 315f;
                        break;
                    case 5: //NE
                        rotationDegrees = 45f;
                        break;
                    case 6: //SW
                        rotationDegrees = 225f;
                        break;
                    case 7: //SE
                        rotationDegrees = 135f;
                        break;
                }
            }

            if ((!upper && mShowLower && mZDimension < 1) || (!upper && mShowLower && mZDimension > 0))
            {
                //Draw Lower
                GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation,
                    MyBase.Lower.Sprite);
                if (tex != null)
                {
                    if (MyBase.Lower.XFrames > 0 && MyBase.Lower.YFrames > 0)
                    {
                        int frameWidth = tex.GetWidth() / MyBase.Lower.XFrames;
                        int frameHeight = tex.GetHeight() / MyBase.Lower.YFrames;
                        GameGraphics.DrawGameTexture(tex,
                            new FloatRect((mLowerFrame % MyBase.Lower.XFrames) * frameWidth,
                                (float) Math.Floor((double) mLowerFrame / MyBase.Lower.XFrames) * frameHeight,
                                frameWidth,
                                frameHeight),
                            new FloatRect(mRenderX - frameWidth / 2, mRenderY - frameHeight / 2, frameWidth,
                                frameHeight),
                            Intersect.Color.White, null, GameBlendModes.None, null, rotationDegrees);
                    }
                }
                int offsetX = MyBase.Lower.Lights[mLowerFrame].OffsetX;
                int offsetY = MyBase.Lower.Lights[mLowerFrame].OffsetY;
                var offset = RotatePoint(new Framework.GenericClasses.Point((int) offsetX, (int) offsetY), new Framework.GenericClasses.Point(0, 0),
                    rotationDegrees + 180);
                GameGraphics.AddLight((int) mRenderX - offset.X,
                    (int) mRenderY - offset.Y, MyBase.Lower.Lights[mLowerFrame].Size,
                    MyBase.Lower.Lights[mLowerFrame].Intensity, MyBase.Lower.Lights[mLowerFrame].Expand,
                    MyBase.Lower.Lights[mLowerFrame].Color);
            }

            if ((upper && mShowUpper && mZDimension != 0) || (upper && mShowUpper && mZDimension == 0))
            {
                //Draw Upper
                GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation,
                    MyBase.Upper.Sprite);
                if (tex != null)
                {
                    if (MyBase.Upper.XFrames > 0 && MyBase.Upper.YFrames > 0)
                    {
                        int frameWidth = tex.GetWidth() / MyBase.Upper.XFrames;
                        int frameHeight = tex.GetHeight() / MyBase.Upper.YFrames;

                        GameGraphics.DrawGameTexture(tex,
                            new FloatRect((mUpperFrame % MyBase.Upper.XFrames) * frameWidth,
                                (float) Math.Floor((double) mUpperFrame / MyBase.Upper.XFrames) * frameHeight,
                                frameWidth,
                                frameHeight),
                            new FloatRect(mRenderX - frameWidth / 2, mRenderY - frameHeight / 2, frameWidth,
                                frameHeight),
                            Intersect.Color.White, null, GameBlendModes.None, null, rotationDegrees);
                    }
                }
                int offsetX = MyBase.Upper.Lights[mUpperFrame].OffsetX;
                int offsetY = MyBase.Upper.Lights[mUpperFrame].OffsetY;
                var offset = RotatePoint(new Framework.GenericClasses.Point((int) offsetX, (int) offsetY), new Framework.GenericClasses.Point(0, 0),
                    rotationDegrees + 180);
                GameGraphics.AddLight((int) mRenderX - offset.X,
                    (int) mRenderY - offset.Y, MyBase.Upper.Lights[mUpperFrame].Size,
                    MyBase.Upper.Lights[mUpperFrame].Intensity, MyBase.Upper.Lights[mUpperFrame].Expand,
                    MyBase.Upper.Lights[mUpperFrame].Color);
            }
        }

        static Framework.GenericClasses.Point RotatePoint(Framework.GenericClasses.Point pointToRotate, Framework.GenericClasses.Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Framework.GenericClasses.Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                     sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                     cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public void Hide()
        {
            Hidden = true;
        }

        public void Show()
        {
            Hidden = false;
        }

        public bool ParentGone()
        {
            if (mParent != null && mParent.IsDisposed())
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            lock (GameGraphics.AnimationLock)
            {
                if (mSound != null)
                {
                    // TODO: I'm not sure that we should really do this so I only commented out Stop()
                    //mSound.Loop = false;
                    mSound.Stop();
                    mSound = null;
                }
                GameGraphics.LiveAnimations.Remove(this);
            }
        }

        public void SetPosition(float worldX, float worldY, int mapx, int mapy, Guid mapId, int dir, int z = 0)
        {
            mRenderX = worldX;
            mRenderY = worldY;
            if (mSound != null)
            {
                mSound.UpdatePosition(mapx, mapy, mapId);
            }
            if (dir > -1) mRenderDir = dir;
            mZDimension = z;
        }

        public void Update()
        {
            if (MyBase != null)
            {
                if (mSound != null)
                {
                    mSound.Update();
                }
                if (mLowerTimer < Globals.System.GetTimeMs() && mShowLower)
                {
                    mLowerFrame++;
                    if (mLowerFrame >= MyBase.Lower.FrameCount)
                    {
                        mLowerLoop--;
                        mLowerFrame = 0;
                        if (mLowerLoop < 0)
                        {
                            if (InfiniteLoop)
                            {
                                mLowerLoop = MyBase.Lower.LoopCount;
                            }
                            else
                            {
                                mShowLower = false;
                            }
                        }
                    }
                    mLowerTimer = Globals.System.GetTimeMs() + MyBase.Lower.FrameSpeed;
                }
                if (mUpperTimer < Globals.System.GetTimeMs() && mShowUpper)
                {
                    mUpperFrame++;
                    if (mUpperFrame >= MyBase.Upper.FrameCount)
                    {
                        mUpperLoop--;
                        mUpperFrame = 0;
                        if (mUpperLoop < 0)
                        {
                            if (InfiniteLoop)
                            {
                                mUpperLoop = MyBase.Upper.LoopCount;
                            }
                            else
                            {
                                mShowUpper = false;
                            }
                        }
                    }
                    mUpperTimer = Globals.System.GetTimeMs() + MyBase.Upper.FrameSpeed;
                }
                if (!mShowLower && !mShowUpper)
                {
                    Dispose();
                }
            }
        }

        public Framework.GenericClasses.Point AnimationSize()
        {
            var size = new Framework.GenericClasses.Point(0, 0);

            GameTexture tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation,
                MyBase.Lower.Sprite);
            if (tex != null)
            {
                if (MyBase.Lower.XFrames > 0 && MyBase.Lower.YFrames > 0)
                {
                    int frameWidth = tex.GetWidth() / MyBase.Lower.XFrames;
                    int frameHeight = tex.GetHeight() / MyBase.Lower.YFrames;
                    if (frameWidth > size.X) size.X = frameWidth;
                    if (frameHeight > size.Y) size.Y = frameHeight;
                }
            }

            tex = Globals.ContentManager.GetTexture(GameContentManager.TextureType.Animation,
                MyBase.Upper.Sprite);
            if (tex != null)
            {
                if (MyBase.Upper.XFrames > 0 && MyBase.Upper.YFrames > 0)
                {
                    int frameWidth = tex.GetWidth() / MyBase.Upper.XFrames;
                    int frameHeight = tex.GetHeight() / MyBase.Upper.YFrames;
                    if (frameWidth > size.X) size.X = frameWidth;
                    if (frameHeight > size.Y) size.Y = frameHeight;
                }
            }

            foreach (var light in MyBase.Lower.Lights)
            {
                if (light != null)
                {
                    if (light.Size + Math.Abs(light.OffsetX) > size.X) size.X = light.Size + light.OffsetX;
                    if (light.Size + Math.Abs(light.OffsetY) > size.Y) size.Y = light.Size + light.OffsetY;
                }
            }

            foreach (var light in MyBase.Upper.Lights)
            {
                if (light != null)
                {
                    if (light.Size + Math.Abs(light.OffsetX) > size.X) size.X = light.Size + light.OffsetX;
                    if (light.Size + Math.Abs(light.OffsetY) > size.Y) size.Y = light.Size + light.OffsetY;
                }
            }

            return size;
        }

        public void SetDir(int dir)
        {
            mRenderDir = dir;
        }
    }
}