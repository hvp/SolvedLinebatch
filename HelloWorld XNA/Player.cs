#region Using Statements


using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using System.Collections.Generic;
using System.Diagnostics;


using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
#endregion


namespace FarseerPhysics.HelloWorld
{
    class Player
    {

#region Fields

        Texture2D walkCycleTex;
        Texture2D idelCycleTex;
        Vector2 playerPostion;
        public  Body _playerRectBody;
        public bool _playerCollison;
        public static Vector2 PlayerPostionIdent;
        public bool isIdel = false;

        public float playerRunSpeed = 5;
        int i = 0; // start frameBlock
        int k = 0; // start frame for idel.
        float ElapsedSeconds;
        float ElpasedSecoundsIdel = 0;
        bool direction = true;
      
        public float DisplacmentLayer3X = 0f; // Moutians
        public float DisplacmentLayer2_5X = 0f; // MidTrees
        public float DisplacmentLayer2X = 0f; // fence

        public float DisplacmentLayer3Y = 0f; // Moutians
        public float DisplacmentLayer2_5Y = 0f; // MidTrees
        public float DisplacmentLayer2Y = 0f; // fence

        public float DisplacmentModifer3 = 0.0001f; // moutian offset.
        public float DisplacmentModifer2_5 = 0.0005f; // middle trees
        public float DisplacmentModifer2 = 0.0008f; // fence

        public float playerrot;
#endregion
        /* Load up player textures, then deifne the physics body */
        public Player(Texture2D Tex, Texture2D Tex2)
        {

            walkCycleTex = Tex;
            idelCycleTex = Tex2;
            Vector2 screenCenter = new Vector2(1200 / 2, 720 / 2);

            playerPostion = (screenCenter) / 64;
                
            _playerRectBody = BodyFactory.CreateRectangle(Game1._world, 80f / 64f,89f / 64f,1f,playerPostion);
            _playerRectBody.BodyType = BodyType.Dynamic;
            _playerRectBody.Mass = 15f;
            _playerRectBody.Friction = 0.5f;
            _playerRectBody.Restitution = 0f;
            _playerRectBody.BodyId = 1;
            
        }

        public void Update(GameTime gameTime)
        {
            /* frame jump*/
            PlayerFrameUpdate(gameTime);


            _playerRectBody.FixedRotation = true;
           

           


            _playerRectBody.OnCollision += OnCollison;
            _playerRectBody.OnSeparation += OnSeparation;

            PlayerPostionIdent = _playerRectBody.Position; // quick work around should work ok.

            /* Add our pixel per secound to our layers then divieided by them */

          

            if (_playerRectBody.LinearVelocity.X < 1 && _playerRectBody.LinearVelocity.X > -1)
            {
                isIdel = true;

            }
            else
            {
                isIdel = false;
            }

            if (isIdel)
            {
                ElpasedSecoundsIdel += gameTime.ElapsedGameTime.Milliseconds;

                if (ElpasedSecoundsIdel > 100)
                {
                    ElpasedSecoundsIdel = 0;
                    k += 100;


                    if (k == 1900)
                    {
                     k = 0;
                    }
                }

             
            }

             }

        public void Draw(SpriteBatch batch)
        {
            
            /* for drawing  */
            Vector2 playerPos = _playerRectBody.Position * 64; 
            float PlayerRotation = _playerRectBody.Rotation;
            Vector2 PlayerOrigin = new Vector2(50, 50);

            
            if (direction && !isIdel)
            {
                batch.Draw(walkCycleTex, playerPos,new Rectangle(0, i, 100, 100), Color.White, PlayerRotation, PlayerOrigin, 1f, SpriteEffects.None, 1f);
            }
            else if (!direction && !isIdel)
            {

                batch.Draw(walkCycleTex, playerPos, new Rectangle(0, i, 100, 100), Color.White, PlayerRotation, PlayerOrigin, 1f, SpriteEffects.FlipHorizontally, 1f);
            }
            if (isIdel && direction)
            {

                batch.Draw(idelCycleTex, playerPos, new Rectangle(0, k, 100, 100), Color.White, PlayerRotation, PlayerOrigin, 1f, SpriteEffects.None, 1f);
        
            }

            if (isIdel && !direction)
            {

                batch.Draw(idelCycleTex, playerPos, new Rectangle(0, k, 100, 100), Color.White, PlayerRotation, PlayerOrigin, 1f, SpriteEffects.FlipHorizontally, 1f);

            }

         
           
        }
        private bool OnCollison(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            Body Bod1 = fixtureA.Body;
            Body Bod2 = fixtureB.Body;

            if (Bod1.BodyId == 1 && Bod2.BodyId == 2)
            {
                _playerCollison = true;

            }

            return true;
        }
        private void OnSeparation(Fixture fixtureA, Fixture fixtureB)
        {
            Body body1 = fixtureA.Body;
            Body body2 = fixtureB.Body;
            if (body1.BodyId == 1 && body2.BodyId == 2)
            {
               _playerCollison = false; 
            }
            


        }
        private void PlayerFrameUpdate(GameTime gameTime)
        {
            ElapsedSeconds += (float)gameTime.ElapsedGameTime.Milliseconds;





            if (ElapsedSeconds > 200 - (_playerRectBody.LinearVelocity.X * 20) && direction == true)
            {

                i += 100;
                ElapsedSeconds = 0;
            }
            else if (ElapsedSeconds > 200 - (-1 * (_playerRectBody.LinearVelocity.X * 20)))
            {

                i += 100;
                ElapsedSeconds = 0;
            }



            if (i >= 700)
            {
                i = 0;
            }

            if (_playerRectBody.LinearVelocity.X > 0.1)
            {

                direction = true;
            }
            else if (_playerRectBody.LinearVelocity.X < -0.1)
            {
                direction = false;

            }

        }





    }
}
