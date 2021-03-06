﻿#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.Collections;
using GameName1.Interfaces;
using GameName1.NPCs;
#endregion

namespace GameName1
{

    public class Seizonsha : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private static readonly Random randomGen = new Random();

        private Player[] players;
        private List<GameEntity> entities;
        private Queue<GameEntity> removalQueue;
        private Queue<Spawnable> spawnQueue;
        private HashSet<Collision> collisions;
        private List<AI> AIs;

        private Level currLevel;


		// ALEX
		Vector2 playerMouseDistance; // distance between player and mouse
		//-ALEX

        public Seizonsha()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        



        protected override void Initialize()
        {
            entities = new List<GameEntity>();
            AIs = new List<AI>();
            removalQueue = new Queue<GameEntity>();
            spawnQueue = new Queue<Spawnable>();
            collisions = new HashSet<Collision>();
            currLevel = new Level(this);
            graphics.PreferredBackBufferHeight = Static.SCREEN_HEIGHT;
            graphics.PreferredBackBufferWidth = Static.SCREEN_WIDTH;
            this.players = new Player[4];

            //just for testing -- makes a rectangle
			//Texture2D playerRect = new Texture2D(GraphicsDevice, Static.PLAYER_HEIGHT, Static.PLAYER_WIDTH);
			Texture2D playerRect = Content.Load<Texture2D>("Sprites/player"); 

           
            Color[] data = new Color[Static.PLAYER_HEIGHT * Static.PLAYER_WIDTH];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = Color.Aquamarine;
            }

			//playerRect.SetData(data);
            //will use real sprites eventually..

            players[0] = new Player(this,PlayerIndex.One,playerRect,0,0);

            Spawn(players[0]);
            Spawn(new BasicNPC(this, playerRect, 300, 100, 10, 10));

            base.Initialize();
        }




        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Font");

        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }



        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            
			// ALEX
			IsMouseVisible = true; 
			//-ALEX







            //spawn Spawnables
            while (spawnQueue.Count > 0)
            {
                Spawnable spawn = spawnQueue.Dequeue();
                if (spawn is GameEntity)
                {
 
                    entities.Add((GameEntity)spawn);
                    if (((GameEntity)spawn).isCollidable())
                    {
                        BindEntityToTiles((GameEntity)spawn, true);
                    }

                    if (spawn is AI)
                    {
                        AIs.Add((AI)spawn);
                    }
                }



                spawn.OnSpawn();

            }

            //update all entities including players
            foreach (GameEntity entity in entities)
            {
                entity.UpdateAll();
                if (entity.shouldRemove())
                {
                    if (entity is GameEntity){
                        BindEntityToTiles((GameEntity)entity, false);
                    }
                    removalQueue.Enqueue(entity);
                }

            }


            //remove entities flagged for removal
            while (removalQueue.Count > 0)
            {
                GameEntity remEntity = removalQueue.Dequeue();
                entities.Remove(remEntity);
                if (remEntity is AI)
                {
                    AIs.Remove((AI)remEntity);
                }
            }


            //handle all player input
            foreach (Player player in players)
            {
                if (player == null)
                {
                    continue;
                }

				handlePlayerInput(player);

				// ALEX
				player.alexAngle = (float)Math.Atan2(playerMouseDistance.Y, playerMouseDistance.X); // angle to point
					
				player.alexDirection = new Vector2( (float)Math.Cos(player.alexAngle), (float) Math.Sin(player.alexAngle));
				//-ALEX

       


            }

            //run AI
            foreach (AI ai in AIs)
            {
                ai.AI();
            }


            //execute all collisions

            foreach (Collision collision in collisions)
            {
                collision.execute();
            }
            collisions.Clear();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();


            currLevel.Draw(spriteBatch, 0, 0);
            foreach (GameEntity entity in entities)
            {
                entity.Draw(spriteBatch);
            }

            foreach (Player player in players)
            {
                if (player == null)
                {
                    continue;
                }

				// DISPLAY TEXT FOR LIST OF SKILLS 
                string displaySkills = "L1: " + player.getSkill(Static.PLAYER_L1_SKILL_INDEX).getName() + "\n" +
                    "L2: " + player.getSkill(Static.PLAYER_L2_SKILL_INDEX).getName() + "\n" +
                    "R1: " + player.getSkill(Static.PLAYER_R1_SKILL_INDEX).getName() + "\n" +
					"R2: " + player.getSkill(Static.PLAYER_R2_SKILL_INDEX).getName() + "\n" +


					// ALEX
					"LEFT CLICK: " + player.getSkill(Static.PLAYER_LEFTCLICK_SKILL_INDEX).getName() + "\n" +

					"direction: " + player.alexDirection + "\n";


                spriteBatch.DrawString(spriteFont, displaySkills, new Vector2(50, 50), Color.White);
            }

            //print text
            //spriteBatch.DrawString(spriteFont, "TEXT", new Vector2(50, 50), Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }



        protected void handlePlayerInput(Player player)
        {
            if (GamePad.GetState(player.playerIndex).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (GamePad.GetState(player.playerIndex).ThumbSticks.Left.Y > .5 || Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                player.MoveUp();
                player.rotateToAngle((float)Math.PI / 2);
            }
            if (GamePad.GetState(player.playerIndex).ThumbSticks.Left.X < -.5 || Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                player.MoveLeft();
                player.rotateToAngle((float)Math.PI);

            }
            if (GamePad.GetState(player.playerIndex).ThumbSticks.Left.X > .5 || Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                player.MoveRight();
                player.rotateToAngle((float)0);

            }
            if (GamePad.GetState(player.playerIndex).ThumbSticks.Left.Y < -.5 || Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                player.MoveDown();
                player.rotateToAngle((float)(3 * Math.PI / 2));

            }


            if (GamePad.GetState(player.playerIndex).Buttons.LeftShoulder == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.D1))
            {
                player.L1Button();
            }
            if (GamePad.GetState(player.playerIndex).Triggers.Left > .5f || Keyboard.GetState().IsKeyDown(Keys.D2))
            {
                player.L2Button();
            }
            if (GamePad.GetState(player.playerIndex).Buttons.RightShoulder == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.D3))
            {
                player.R1Button();
            }
            if (GamePad.GetState(player.playerIndex).Triggers.Right > .5f || Keyboard.GetState().IsKeyDown(Keys.D4))
            {
                player.R2Button();
            }

			// ALEX
			MouseState mouse = Mouse.GetState(); 

			playerMouseDistance.X = mouse.X - player.x;	// distance between player and mouse
			playerMouseDistance.Y = mouse.Y - player.y;

			if (mouse.LeftButton == ButtonState.Pressed) {
				player.LeftClick();
			}


			//-ALEX
        }


        public void BindEntityToTiles(GameEntity entity, bool bind)
        {
            for (int i = getTileIndexFromLeftEdgeX(entity.getLeftEdgeX()); i <= getTileIndexFromRightEdgeX(entity.getRightEdgeX()); i++)
            {
                for (int j = getTileIndexFromTopEdgeY(entity.getTopEdgeY()); j <= getTileIndexFromBottomEdgeY(entity.getBottomEdgeY()); j++)
                {
                    Tile currTile = currLevel.getTile(i, j);
                    if (currTile != null) //if within bounds of level
                    {
                        currLevel.getTile(i, j).BindEntity(entity, bind);
                    }
                }
            }
        }

        public Texture2D getTestSprite(Rectangle bounds, Color color)
        {
            Texture2D testRect = new Texture2D(GraphicsDevice, bounds.Height, bounds.Width);

            Color[] data = new Color[bounds.Height * bounds.Width];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = color;
            }

            testRect.SetData(data);
            return testRect;
        }


        public void damageArea(Rectangle bounds, int amount, int damageType){
            foreach (GameEntity entity in getEntitiesInBounds(bounds)){
                entity.damage(amount, damageType);
            }
        }

        private void moveGameEntityWithoutCollision(GameEntity entity, int x, int y)
        {
            entity.x = x;
            entity.y = y;
            entity.hitbox.X = x;
            entity.hitbox.Y = y;
        }

        public void moveGameEntity(GameEntity entity, int dx, int dy)
        {

            BindEntityToTiles(entity, false);

            if (!entity.isCollidable()) //skip collision detection
            {
                moveGameEntityWithoutCollision(entity, entity.x + dx, entity.y + dy);
                BindEntityToTiles(entity, true);
                return;
            }


            bool wallCollision = false;

            //calculate number of tiles distance covers
            int tilesX = (int)Math.Floor((float)Math.Abs(dx) / Static.TILE_WIDTH) + 1;
            int tilesY = (int)Math.Floor((float)Math.Abs(dy) / Static.TILE_HEIGHT) + 1;

            //get entity bounds
            int leftEdgeTile = getTileIndexFromLeftEdgeX(entity.getLeftEdgeX());
            int rightEdgeTile = getTileIndexFromRightEdgeX(entity.getRightEdgeX());
            int topEdgeTile = getTileIndexFromTopEdgeY(entity.getTopEdgeY());
            int bottomEdgeTile = getTileIndexFromBottomEdgeY(entity.getBottomEdgeY());

            int distanceToTravelX = dx;
            int distanceToTravelY = dy;

            //find distance to level boundary in movement direction and see if it is less move amount
            //figure out how many tiles your movement translates to in each direction
            //scan tiles in front of you to find closest obstacle in that direction
            //final movement is min of original movement and distance to obstacle


            if (dx > 0)
            {
                //right

                int distanceToBoundary = currLevel.GetTilesHorizontal() * Static.TILE_WIDTH - entity.getRightEdgeX();
                if (distanceToBoundary < distanceToTravelX)
                {
                    distanceToTravelX = distanceToBoundary;
                    //wallCollision = true;
                }

                int tilesToScanX = tilesX;

                if (rightEdgeTile + tilesToScanX > currLevel.GetTilesHorizontal() - 1)
                {
                    tilesToScanX = currLevel.GetTilesHorizontal() - 1 - rightEdgeTile;
                }


                for (int i = 1; i <= tilesToScanX; i++)
                {
                    for (int j = topEdgeTile; j <= bottomEdgeTile; j++)
                    {
                        Tile currTile = currLevel.getTile(rightEdgeTile + i, j);
                        if (currTile.isObstacle())
                        {
                            int distanceToTile = currTile.x - entity.getRightEdgeX();
                            if (distanceToTile < distanceToTravelX)
                            {
                                distanceToTravelX = distanceToTile;
                                wallCollision = true;
                                break;
                            }
                        }

                        GameEntity closest = null;
                        foreach (GameEntity tileEntity in currTile.getEntities()) 
                        {
                            if (tileEntity.getLeftEdgeX() - entity.getRightEdgeX() < distanceToTravelX)
                            {
                                if (tileEntity.OverlapsY(entity))
                                {
                                    distanceToTravelX = tileEntity.getLeftEdgeX() - entity.getRightEdgeX();
                                    closest = tileEntity;
                                }
                            }
                        }
                        if (closest != null)
                        {
                            collisions.Add(new Collision(entity, closest));
                        }
                    }
                }


            }
            else if (dx < 0)
            {
                //left


                int distanceToBoundary = -1 * entity.getLeftEdgeX();
                if (distanceToBoundary > distanceToTravelX)
                {
                    distanceToTravelX = distanceToBoundary;
                    //wallCollision = true;
                }

                int tilesToScanX = tilesX;

                if (leftEdgeTile - tilesToScanX < 0)
                {
                    tilesToScanX = leftEdgeTile;
                }


                for (int i = 1; i <= tilesToScanX; i++)
                {
                    for (int j = topEdgeTile; j <= bottomEdgeTile; j++)
                    {
                        Tile currTile = currLevel.getTile(leftEdgeTile - i, j);
                        if (currTile.isObstacle())
                        {
                            int distanceToTile = (currTile.x + Static.TILE_WIDTH) - entity.getLeftEdgeX();
                            if (distanceToTile > distanceToTravelX)
                            {
                                distanceToTravelX = distanceToTile;
                                wallCollision = true;
                                break;
                            }
                        }

                        GameEntity closest = null;
                        foreach (GameEntity tileEntity in currTile.getEntities())
                        {
                            if (tileEntity.getRightEdgeX() - entity.getLeftEdgeX() > distanceToTravelX)
                            {
                                if (tileEntity.OverlapsY(entity))
                                {
                                    distanceToTravelX = tileEntity.getRightEdgeX() - entity.getLeftEdgeX();
                                    closest = tileEntity;
                                }
                            }
                        }
                        if (closest != null)
                        {
                            collisions.Add(new Collision(entity, closest));
                        }

                    }

                }
            }

            entity.x = entity.x + distanceToTravelX;
            entity.hitbox.Offset(distanceToTravelX, 0);
            leftEdgeTile = getTileIndexFromLeftEdgeX(entity.getLeftEdgeX());
            rightEdgeTile = getTileIndexFromRightEdgeX(entity.getRightEdgeX());


            if (dy > 0)
            { //down

                int distanceToBoundary = currLevel.GetTilesVertical() * Static.TILE_HEIGHT - entity.getBottomEdgeY();
                if (distanceToBoundary < distanceToTravelY)
                {
                    distanceToTravelY = distanceToBoundary;
                    //wallCollision = true;
                }


                int tilesToScanY = tilesY;

                if (bottomEdgeTile + tilesToScanY > currLevel.GetTilesVertical() - 1)
                {
                    tilesToScanY = currLevel.GetTilesVertical() - 1 - bottomEdgeTile;
                }

                for (int i = 1; i <= tilesToScanY; i++)
                {
                    for (int j = leftEdgeTile; j <= rightEdgeTile; j++)
                    {
                        Tile currTile = currLevel.getTile(j, bottomEdgeTile + i);
                        if (currTile.isObstacle())
                        {
                            int distanceToTile = currTile.y - entity.getBottomEdgeY();
                            if (distanceToTile < distanceToTravelY)
                            {
                                distanceToTravelY = distanceToTile;
                                wallCollision = true;
                                break;
                            }
                        }

                        GameEntity closest = null;
                        foreach (GameEntity tileEntity in currTile.getEntities())
                        {
                            if (tileEntity.getTopEdgeY() - entity.getBottomEdgeY() < distanceToTravelY)
                            {
                                if (tileEntity.OverlapsX(entity))
                                {
                                    distanceToTravelY = tileEntity.getTopEdgeY() - entity.getBottomEdgeY();
                                    closest = tileEntity;
                                }
                            }
                        }
                        if (closest != null)
                        {
                            
                            collisions.Add(new Collision(entity, closest));
                        }
                    }
                }

            }
            else if (dy < 0)
            { //up


                int distanceToBoundary = -1 * entity.getTopEdgeY();
                if (distanceToBoundary > distanceToTravelY)
                {
                    distanceToTravelY = distanceToBoundary;
                    //wallCollision = true;
                }

                int tilesToScanY = tilesY;

                if (topEdgeTile - tilesToScanY < 0)
                {
                    tilesToScanY = topEdgeTile;
                }


                for (int i = 1; i <= tilesToScanY; i++)
                {
                    for (int j = leftEdgeTile; j <= rightEdgeTile; j++)
                    {
                        Tile currTile = currLevel.getTile(j, topEdgeTile - i);
                        if (currTile.isObstacle())
                        {
                            int distanceToTile = (currTile.y + Static.TILE_HEIGHT) - entity.getTopEdgeY();
                            if (distanceToTile > distanceToTravelY)
                            {
                                    distanceToTravelY = distanceToTile;
                                    wallCollision = true;
                                    break;
                            }
                        }

                        GameEntity closest = null;
                        foreach (GameEntity tileEntity in currTile.getEntities())
                        {
                            if (tileEntity.getBottomEdgeY() - entity.getTopEdgeY() > distanceToTravelY)
                            {
                                if (tileEntity.OverlapsX(entity))
                                {
                                    distanceToTravelY = tileEntity.getBottomEdgeY() - entity.getTopEdgeY();
                                    closest = tileEntity;
                                }
                            }
                        }
                        if (closest != null)
                        {
                            collisions.Add(new Collision(entity, closest));
                        }
                    }

                }

            }

            entity.y = entity.y + distanceToTravelY;
            entity.hitbox.Offset(0, distanceToTravelY);

            if (wallCollision)
            {
                entity.collideWithWall();
            }

            BindEntityToTiles(entity, true);

        }




        public List<GameEntity> getEntitiesInBounds(Rectangle bounds)
        {

            //hash set so there will be no duplicates
            HashSet<GameEntity> returnSet = new HashSet<GameEntity>();
            for (int i = getTileIndexFromLeftEdgeX(bounds.Left); i <= getTileIndexFromRightEdgeX(bounds.Right); i++)
            {
                for (int j = getTileIndexFromTopEdgeY(bounds.Top); j <= getTileIndexFromBottomEdgeY(bounds.Bottom); j++)
                {
                    Tile currTile = currLevel.getTile(i, j);
                    if (currTile != null)
                    {
                        foreach (GameEntity entity in currTile.getEntities())
                        {
                            returnSet.Add(entity);
                        }
                    }
                }
            }

            List<GameEntity> returnList = new List<GameEntity>();
            foreach (GameEntity entity in returnSet)
            {
                returnList.Add(entity);
            }

            return returnList;
        }

        public int getTileIndexFromLeftEdgeX(int x)
        {
            return (int)Math.Floor((float)x / Static.TILE_WIDTH);
        }

        public int getTileIndexFromRightEdgeX(int x)
        {
            return (int)Math.Ceiling(((float)x / Static.TILE_WIDTH)) - 1;
        }

        public int getTileIndexFromTopEdgeY(int y)
        {
            return (int)Math.Floor((float)y / Static.TILE_HEIGHT);
        }

        public int getTileIndexFromBottomEdgeY(int y)
        {
            return (int)Math.Ceiling(((float)y / Static.TILE_HEIGHT)) - 1;
        }

        public void Spawn(Spawnable spawn)
        {
            spawnQueue.Enqueue(spawn);
        }

        public SpriteFont getSpriteFont()
        {
            return spriteFont;
        }

        public Rectangle getLevelBounds()
        {
            return currLevel.getBounds();
        }



    }
}
