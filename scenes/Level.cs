﻿using Otter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD31 {
	class Level : Scene {

		private static Image background = new Image("assets/gfx/BG.png");
		private static Image corners = new Image("assets/gfx/Corners.png");

		private static Surface ambientLighting = new Surface(1920, 1080);
		private static Surface lightSurface = new Surface(1920, 1080);
		private static Surface darknessSurface = new Surface(1920, 1080);

		private static Image lightTexture1 = new Image("assets/gfx/lightTexture.png");
		private static List<Image> lightTextures = new List<Image>();

		public static List<Light> lights = new List<Light>();
		public static List<Light> darkness = new List<Light>();

		public static Player player;

		private bool finished = false;

		private static Shader shader = new Shader(ShaderType.Fragment, "assets/shaders/water_wave.frag");

		public Level() {
			// Set up background
			AddGraphic(background);
			background.Scroll = 0;
			AddGraphic(corners);

			background.Shader = shader;

			lightTexture1.CenterOrigin();

			lightTextures.Add(lightTexture1);

			lights.Clear();
			darkness.Clear();

			player = null;
		}

		public override void Begin() {
			base.Begin();

			// Add the player
			player = Add(new Player(1920 >> 1, 1080 >> 1, Global.PlayerOne));

			Add(new EnemySpawner(player));

			var explosion = Add(new Explosion(1920 >> 1, 1080 >> 1));
			explosion.SetAlpha(2.0f, 1.0f, 0.0f);
			explosion.SetRadius(2.0f, 100.0f, 580.0f, 560.0f, 480.0f);

			// Create the four corners
			CreateCorners();

			// Add dat surface
			Game.AddSurface(ambientLighting);
			Game.AddSurface(lightSurface);
			Game.AddSurface(darknessSurface);
		}

		public override void End() {
			Game.RemoveSurface(ambientLighting);
			Game.RemoveSurface(lightSurface);
			Game.RemoveSurface(darknessSurface);
		}

		private void CreateCorners() {
			// Entities
			var corner1 = Add(new Corner(-40, -40));
			var corner2 = Add(new Corner(-40, Game.Height - (int)Corner.size.Y + 40));
			var corner3 = Add(new Corner(Game.Width - (int)Corner.size.X + 40, Game.Height - (int)Corner.size.Y + 40));
			var corner4 = Add(new Corner(Game.Width - (int)Corner.size.X + 40, -40));

			// Lights
			#region Lights
			/*var light1 = new Light();
			light1.entity = corner1;
			light1.SetColor(new Color("008080"));
			light1.SetRadius(300.0f);
			light1.SetAlpha(0.5f);
			light1.SetOffset(30, 30);
			lights.Add(light1);

			var light2 = new Light();
			light2.entity = corner2;
			light2.SetColor(new Color("008080"));
			light2.SetRadius(300.0f);
			light2.SetAlpha(0.5f);
			light2.SetOffset(30, -30);
			lights.Add(light2);

			var light3 = new Light();
			light3.entity = corner3;
			light3.SetColor(new Color("008080"));
			light3.SetRadius(300.0f);
			light3.SetAlpha(0.5f);
			light3.SetOffset(-30, -30);
			lights.Add(light3);

			var light4 = new Light();
			light4.entity = corner4;
			light4.SetColor(new Color("008080"));
			light4.SetRadius(300.0f);
			light4.SetAlpha(0.5f);
			light4.SetOffset(30, -30);
			lights.Add(light4);*/
			#endregion
		}

		public override void Update() {
			if (Input.KeyPressed(Key.Escape)) {
				GameOver();
			}

			Screenshaker.Shake();
			CameraX = Screenshaker.CameraX;
			CameraY = Screenshaker.CameraY;

			Global.backgroundTimer += Game.DeltaTime;
			background.Shader.SetParameter("timer", Global.backgroundTimer);

			foreach (Light light in lights) {
				light.Update(Game.RealDeltaTime * 0.001f);
			}

			foreach (Light light in darkness) {
				light.Update(Game.RealDeltaTime * 0.001f);
			}
		}

		public override void Render() {
			base.Render();

			var texture = Game.Surface.GetTexture();

			ambientLighting.Fill(new Color("BCBCBC"));
			ambientLighting.Blend = BlendMode.Multiply;

			darknessSurface.Fill(new Color("FFFFFF"));
			darknessSurface.Blend = BlendMode.Multiply;

			foreach (Light light in lights) {
				var i = light.Image;
				lightTextures[i].Color = light.Color;
				lightTextures[i].Alpha = light.Alpha;
				lightTextures[i].Scale = light.Scale;
				lightSurface.Draw(lightTextures[i], light.X, light.Y);
			}

			foreach (Light light in darkness) {
				var i = light.Image;
				lightTextures[i].Color = light.Color;
				lightTextures[i].Alpha = light.Alpha;
				lightTextures[i].Scale = light.Scale;
				darknessSurface.Draw(lightTextures[i], light.X, light.Y);
			}

			lightSurface.Blend = BlendMode.Add;
		}

		public void Victory() {
			Game.Coroutine.Start(Finish("assets/gfx/victory.png"));
		}

		public void GameOver() {
			Game.Coroutine.Start(Finish("assets/gfx/gameover.png"));
		}

		IEnumerator Finish(string name) {
			if (!finished) {
				finished = true;

				player.ApplyDamage(1000);

				// Show message
				Add(new Logo(960, 200, name));

				// Fade out all the things
				yield return EnemySpawner.Instance.RemoveAllEnemies();

				var canContinue = false;
				while (!canContinue) {
					canContinue = Global.PlayerOne.Controller.Cross.Pressed || Input.KeyPressed(Key.Space) || Input.KeyPressed(Key.Escape);
					yield return 0;
				}

				// Switch back to menu
				Game.SwitchScene(new MainMenu());
			}
		}

	}
}
