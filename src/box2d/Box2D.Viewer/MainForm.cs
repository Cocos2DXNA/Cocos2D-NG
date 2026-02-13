using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Box2D;
using Box2D.Common;
using Box2D.Dynamics;
using Box2D.TestBed;

namespace Box2D.Viewer
{
    public sealed class MainForm : Form
    {
        private readonly ComboBox _scenarioPicker;
        private readonly Button _resetButton;
        private readonly WorldView _worldView;
        private readonly Timer _timer;

        private readonly Dictionary<string, Func<WorldSession>> _sessions;
        private b2World _world;
        private Test _testbed;
        private readonly Settings _testSettings;

        public MainForm()
        {
            Text = "Box2D Viewer";
            ClientSize = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            _sessions = BuildSessions();
            _testSettings = new Settings();

            _scenarioPicker = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 320
            };
            _scenarioPicker.Items.AddRange(_sessions.Keys.Cast<object>().ToArray());
            _scenarioPicker.SelectedIndexChanged += (_, __) => ResetWorld();
            _resetButton = new Button
            {
                Text = "Reset",
                AutoSize = true
            };
            _resetButton.Click += (_, __) => ResetWorld();

            FlowLayoutPanel toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(10, 8, 10, 8)
            };
            toolbar.Controls.Add(_scenarioPicker);
            toolbar.Controls.Add(_resetButton);

            _worldView = new WorldView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 22, 25)
            };
            _worldView.MouseDown += OnWorldViewMouseDown;
            _worldView.MouseUp += OnWorldViewMouseUp;
            _worldView.MouseMove += OnWorldViewMouseMove;

            Controls.Add(_worldView);
            Controls.Add(toolbar);

            _scenarioPicker.SelectedIndex = 0;

            _timer = new Timer { Interval = 16 };
            _timer.Tick += (_, __) => StepWorld();
            _timer.Start();
        }

        private void ResetWorld()
        {
            string key = _scenarioPicker.SelectedItem?.ToString() ?? string.Empty;
            if (!_sessions.TryGetValue(key, out var factory))
            {
                return;
            }

            WorldSession session = factory();
            _testbed = session.Test;
            _world = session.World;
            _worldView.World = _world;
            _worldView.Invalidate();
            _worldView.Focus();
        }

        private void StepWorld()
        {
            if (_world == null)
            {
                return;
            }

            if (_testbed != null)
            {
                _testbed.DebugDraw?.BeginFrame();
                _testbed.Step(_testSettings);
                _worldView.OverlayText = _testbed.DebugDraw?.GetTextSnapshot();
                _worldView.DebugSegments = _testbed.DebugDraw?.GetSegmentSnapshot();
                _worldView.DebugPoints = _testbed.DebugDraw?.GetPointSnapshot();
            }
            else
            {
                _world.Step(1.0f / 60.0f, 8, 3);
                _worldView.OverlayText = null;
                _worldView.DebugSegments = null;
                _worldView.DebugPoints = null;
            }
            _worldView.Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_testbed == null)
            {
                base.OnKeyDown(e);
                return;
            }

            char key = ToTestbedKey(e.KeyCode);
            if (key != '\0')
            {
                _testbed.Keyboard(key);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_testbed == null)
            {
                base.OnKeyUp(e);
                return;
            }

            char key = ToTestbedKey(e.KeyCode);
            if (key != '\0')
            {
                _testbed.KeyboardUp(key);
            }
        }

        private void OnWorldViewMouseDown(object sender, MouseEventArgs e)
        {
            if (_testbed == null)
            {
                return;
            }

            b2Vec2 worldPoint = _worldView.ScreenToWorld(e.Location);
            _testbed.MouseDown(worldPoint);
        }

        private void OnWorldViewMouseUp(object sender, MouseEventArgs e)
        {
            if (_testbed == null)
            {
                return;
            }

            b2Vec2 worldPoint = _worldView.ScreenToWorld(e.Location);
            _testbed.MouseUp(worldPoint);
        }

        private void OnWorldViewMouseMove(object sender, MouseEventArgs e)
        {
            if (_testbed == null)
            {
                return;
            }

            b2Vec2 worldPoint = _worldView.ScreenToWorld(e.Location);
            _testbed.MouseMove(worldPoint);
        }

        private static char ToTestbedKey(Keys keyCode)
        {
            if (keyCode >= Keys.A && keyCode <= Keys.Z)
            {
                return char.ToLowerInvariant((char)keyCode);
            }

            if (keyCode >= Keys.D0 && keyCode <= Keys.D9)
            {
                return (char)('0' + (keyCode - Keys.D0));
            }

            if (keyCode >= Keys.NumPad0 && keyCode <= Keys.NumPad9)
            {
                return (char)('0' + (keyCode - Keys.NumPad0));
            }

            return keyCode switch
            {
                Keys.Oemcomma => ',',
                Keys.OemMinus => '-',
                Keys.OemPeriod => '.',
                _ => '\0'
            };
        }

        private static Dictionary<string, Func<WorldSession>> BuildSessions()
        {
            var sessions = new Dictionary<string, Func<WorldSession>>
            {
                { "Scenario: Stack Of Boxes", CreateScenarioSession(CreateStackWorld) },
                { "Scenario: Gravity Roller Coaster", CreateScenarioSession(CreateRollerCoasterWorld) },
                { "Scenario: Projectile (-Y Gravity)", CreateScenarioSession(CreateProjectileWorld) }
            };

            foreach (Type type in GetTestbedTypes())
            {
                string key = $"Testbed: {type.Name}";
                sessions[key] = () => CreateTestbedSession(type);
            }

            return sessions;
        }

        private static Func<WorldSession> CreateScenarioSession(Func<b2World> factory)
        {
            return () => new WorldSession { World = factory() };
        }

        private static WorldSession CreateTestbedSession(Type testType)
        {
            var test = (Test)Activator.CreateInstance(testType);
            return new WorldSession { World = test.World, Test = test };
        }

        private static IEnumerable<Type> GetTestbedTypes()
        {
            Type baseType = typeof(Test);
            return baseType.Assembly
                .GetTypes()
                .Where(t => t != baseType && baseType.IsAssignableFrom(t) && !t.IsAbstract)
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .Where(t => t.Namespace == "Box2D.TestBed.Tests")
                .OrderBy(t => t.Name);
        }

        private static b2World CreateStackWorld()
        {
            return Box2DScenarios.CreateStackOfBoxes(out _, out _);
        }

        private static b2World CreateRollerCoasterWorld()
        {
            return Box2DScenarios.CreateRollerCoaster(out _);
        }

        private static b2World CreateProjectileWorld()
        {
            return Box2DScenarios.CreateProjectile(out _);
        }

        private sealed class WorldSession
        {
            public b2World World;
            public Test Test;
        }
    }
}
