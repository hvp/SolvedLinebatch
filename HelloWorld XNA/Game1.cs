using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using FarseerPhysics.Controllers;

using System.Collections.Generic;
using System.Diagnostics;

using System.Xml;
using System.Xml.Serialization;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;


namespace FarseerPhysics.HelloWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 
    struct Line
    {
        public Vector2 _start;
        public Vector2 _end;
    };

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private SpriteFont _font;

        public static World _world;

        private Body _circleBody;
        private Body _groundBody;

        private Texture2D _circleSprite;
        private Texture2D _groundSprite;

        Texture2D _house;
        
        Texture2D _houseColoums;

       

        bool _first_point;
        Vector2 _point1;
        MouseState _last_state;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             

        Player playerOne;
        List<Line> _lines;
        List<Body> _ramps;
        // Simple camera controls
        private Matrix _view;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;
        int i = 0;

#if !XBOX360
        const string Text = "Press A or D to rotate the ball\n" +
                            "Press Space to jump\n" +
                            "Press Shift + W/S/A/D to move the camera";
#else
                const string Text = "Use left stick to move\n" +
                                    "Use right stick to move camera\n" +
                                    "Press A to jump\n";
#endif
        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1 meters equals 64 pixels here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float MeterInPixels = 64f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            IsMouseVisible = true;
        
            _lines = new List<Line>();
            _ramps = new List<Body>();
            
            _last_state = Mouse.GetState();
            _first_point = true;
            Content.RootDirectory = "Content";
          
            _world = new World(new Vector2(0, 20));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = Vector2.Zero;

            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f,
                                                _graphics.GraphicsDevice.Viewport.Height / 2f);
            LineBatch.Init(GraphicsDevice);
            _batch = new SpriteBatch(_graphics.GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");
            playerOne = new Player(Content.Load<Texture2D>("CycleStrip"), Content.Load<Texture2D>("_mcIdleV2"));
            // Load sprites
            _circleSprite = Content.Load<Texture2D>("circleSprite"); //  96px x 96px => 1.5m x 1.5m
            _groundSprite = Content.Load<Texture2D>("groundSprite"); // 512px x 64px =>   8m x 1m
            _house = Content.Load<Texture2D>("_townHallRough");
    
            _houseColoums = Content.Load<Texture2D>("_townHallRoughColoums");
            /* Circle */
            // Convert screen center from pixels to meters
            Vector2 circlePosition = (_screenCenter / MeterInPixels) + new Vector2(0, -1.5f);

            // Create the circle fixture
            _circleBody = BodyFactory.CreateCircle(_world, 96f / (2f * MeterInPixels), 1f, circlePosition);
            _circleBody.BodyType = BodyType.Dynamic;

            // Give it some bounce and friction
            _circleBody.Restitution = 0.3f;
            _circleBody.Friction = 0.5f;

            /* Ground */
            Vector2 groundPosition = (_screenCenter / MeterInPixels) + new Vector2(0, 1.25f);

            // Create the ground fixture
            _groundBody = BodyFactory.CreateRectangle(_world, 512f / MeterInPixels, 64f / MeterInPixels, 1f, groundPosition);
            _groundBody.IsStatic = true;
            _groundBody.Restitution = 0.3f;
            _groundBody.Friction = 0.5f;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleGamePad();
            HandleKeyboard();
            playerOne.Update(gameTime);
            if (Mouse.GetState().LeftButton == ButtonState.Pressed
               && _last_state.LeftButton == ButtonState.Released)
            {
                if (_first_point) // If first point then save it
                {
                    _point1 = new Vector2(Mouse.GetState().X - _cameraPosition.X, Mouse.GetState().Y - _cameraPosition.Y);
                    _first_point = false;
                }
                else // Otherwise add it to list
                {
                    Line l = new Line();
                    l._start = _point1;
                    l._end = new Vector2(Mouse.GetState().X - _cameraPosition.X, Mouse.GetState().Y - _cameraPosition.Y);
                    _lines.Add(l);

                    _ramps.Add(BodyFactory.CreateEdge(_world, l._start / 64, l._end / 64));
                    _ramps[i].Friction = 10.5f; // friction vaules seem to have no affect.

                    i++;

                    _first_point = true;
                    _point1 = Vector2.Zero;
                }
            }

            _last_state = Mouse.GetState();
            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        private void HandleGamePad()
        {
            GamePadState padState = GamePad.GetState(0);

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                if (padState.Buttons.A == ButtonState.Pressed && _oldPadState.Buttons.A == ButtonState.Released)
                    _circleBody.ApplyLinearImpulse(new Vector2(0, -10));

                _circleBody.ApplyForce(padState.ThumbSticks.Left);
                _cameraPosition.X -= padState.ThumbSticks.Right.X;
                _cameraPosition.Y += padState.ThumbSticks.Right.Y;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) * Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

                _oldPadState = padState;
            }
        }

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();

            // Switch between circle body and camera control
            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
            {
               
                // Move camera
                if (state.IsKeyDown(Keys.A))
                    _cameraPosition.X += 1.5f;

                if (state.IsKeyDown(Keys.D))
                    _cameraPosition.X -= 1.5f;

                if (state.IsKeyDown(Keys.W))
                    _cameraPosition.Y += 1.5f;

                if (state.IsKeyDown(Keys.S))
                    _cameraPosition.Y -= 1.5f;

                _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) *
                        Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));
            }
            else
            {
              
                if (state.IsKeyDown(Keys.A) && playerOne._playerRectBody.LinearVelocity.X > -5)
                {
                    playerOne._playerRectBody.ApplyLinearImpulse(new Vector2(-5, 0));
                }
                 //   _circleBody.ApplyTorque(-10);

                if (state.IsKeyDown(Keys.D) && playerOne._playerRectBody.LinearVelocity.X < 5 )
                {

                    playerOne._playerRectBody.ApplyLinearImpulse(new Vector2(5, 0));
                }
                if (state.IsKeyDown(Keys.G))
                {
                    /*
                    XmlSerializer serializer = new XmlSerializer(typeof(List<RampData>));
                    TextWriter tw = new StreamWriter("_rampPostionLATEST.xml");
                    serializer.Serialize(tw, _RampDataList);
                    tw.Close();
                */
                }   

                if (state.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
                    playerOne._playerRectBody.ApplyLinearImpulse(new Vector2(0, -20));
            }

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            /* Circle position and rotation */
            // Convert physics position (meters) to screen coordinates (pixels)
            Vector2 circlePos = _circleBody.Position * MeterInPixels;
            float circleRotation = _circleBody.Rotation;

            /* Ground position and origin */
            Vector2 groundPos = _groundBody.Position * MeterInPixels;
            Vector2 groundOrigin = new Vector2(_groundSprite.Width / 2f, _groundSprite.Height / 2f);

            // Align sprite center to body position
            Vector2 circleOrigin = new Vector2(_circleSprite.Width / 2f, _circleSprite.Height / 2f);

            _batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, _view);
      _batch.Draw(_house, new Vector2(100, -465), Color.White); 
          //  _batch.Draw(_circleSprite, circlePos, null, Color.White, circleRotation, circleOrigin, 1f, SpriteEffects.None, 0f);

            playerOne.Draw(_batch);
      _batch.Draw(_houseColoums, new Vector2(100, -465), Color.White);   
            //Draw circle
        
            //Draw ground
            _batch.Draw(_groundSprite, groundPos, null, Color.White, 0f, groundOrigin, 1f, SpriteEffects.None, 0f);
            if (!_first_point && _point1 != Vector2.Zero)
            {
                LineBatch.DrawLine(_batch,
                                   Color.White,
                                   _point1,
                                   new Vector2(Mouse.GetState().X - _cameraPosition.X, Mouse.GetState().Y - _cameraPosition.Y));
            }
            // Draw all lines in list
            for (int i = 0; i < _lines.Count; i++)
            {
                LineBatch.DrawLine(_batch,
                                   Color.Black,
                                   _lines[i]._start,
                                   _lines[i]._end);
            }
            _batch.DrawString(_font, _cameraPosition.ToString(), new Vector2(100, 100), Color.White);

         
            _batch.End();
          

        
            base.Draw(gameTime);
        }
    }
}