using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DobotControl.CPlusDll;
using HelixToolkit.Wpf;
using HelixToolkit.SharpDX;
using System.Windows.Media.Media3D;
using System.IO;
using SharpDX;
using HelixToolkit.Wpf.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;

namespace DobotControl
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {

        private byte isJoint = (byte)0;
        private bool isConnectted = false;
        private JogCmd currentCmd;
        private Pose pose = new Pose();
        private System.Timers.Timer posTimer = new System.Timers.Timer();


        private List<String> inital_filepath = new List<String>();

        //new coordinate of aritculator
        private Point3D articulator_coor = new Point3D(0,0,0);
        //self-define origin after set origin
        private Point3D sd_origin = new Point3D(0, 0, 0);


        private List<HelixToolkit.Wpf.SharpDX.GeometryModel3D> m_group = new List<HelixToolkit.Wpf.SharpDX.GeometryModel3D>();

        public MainWindow()
        {
            InitializeComponent();

            Rect3D initView = new Rect3D(new Point3D(0, 0, 0), new Size3D(100, 100, 100));
            ResetCameraPosition(initView);
            LoadModel(".\\SetupParts\\articulator2.stl");
            LoadModel(".\\SetupParts\\calibratePart.stl");
            PhongMaterial material = new PhongMaterial
            {
                ReflectiveColor = SharpDX.Color.Black,
                AmbientColor = new SharpDX.Color(0.0f, 0.0f, 0.0f, 1.0f),
                EmissiveColor = SharpDX.Color.Black,
                SpecularColor = new SharpDX.Color(90, 90, 90, 255),
                SpecularShininess = 60,
                DiffuseColor = SharpDX.Color.Red
            };
         
            MeshGeometryModel3D model = ModelGroup.Children[1] as MeshGeometryModel3D;
            model.Material = material;
            


        }
        public MainWindow(String[] filepath)
        {
     
           

            InitializeComponent();
            for (int i = 0; i != filepath.Length; ++i)
            {
                MessageBox.Show(filepath[i]);
            }
            Rect3D initView = new Rect3D(new Point3D(0, 0, 0), new Size3D(100, 100, 100));
            ResetCameraPosition(initView);
            LoadModel(".\\SetupParts\\articulator2.stl");
           LoadModel(".\\SetupParts\\calibratePart.stl");
            PhongMaterial material = new PhongMaterial
            {
                ReflectiveColor = SharpDX.Color.Black,
                AmbientColor = new SharpDX.Color(0.0f, 0.0f, 0.0f, 1.0f),
                EmissiveColor = SharpDX.Color.Black,
                SpecularColor = new SharpDX.Color(90, 90, 90, 255),
                SpecularShininess = 60,
                DiffuseColor = SharpDX.Color.Red
            };

            MeshGeometryModel3D model = ModelGroup.Children[1] as MeshGeometryModel3D;
            model.Material = material;
        }
        ~MainWindow()
        {
            DobotDll.DisconnectDobot();
        }
        private void StartGetPose()
        {
            posTimer.Elapsed += new System.Timers.ElapsedEventHandler(PosTimer_Tick);
            posTimer.Interval = 100;
            posTimer.Start();
        }
        private void StartDobot()
        {
            StringBuilder fwType = new StringBuilder(60);
            StringBuilder version = new StringBuilder(60);
            int ret = DobotDll.ConnectDobot("", 115200, fwType, version);
            // start connect
            if (ret != (int)DobotConnect.DobotConnect_NoError)
            {
                Msg("Connect error", MsgInfoType.Error);
                MessageBox.Show("Connect Error!");
                return;
            }
            Msg("Connect success", MsgInfoType.Info);
            MessageBox.Show("Connect Success!");

            isConnectted = true;
            DobotDll.SetCmdTimeout(3000);

            // Must set when sensor is not exist
            //DobotDll.ResetPose(true, 45, 45);

            // Get name
            string deviceName = "Dobot Magician";
            DobotDll.SetDeviceName(deviceName);

            StringBuilder deviceSN = new StringBuilder(64);
            DobotDll.GetDeviceName(deviceSN, 64);

            StartGetPose();

            //  SetParam();

            //EIOTest();

            //ARCTest();

            //  AlarmTest();
            EndTypeParams e = new EndTypeParams();
            DobotDll.GetEndEffectorParams(ref e);
            MessageBox.Show(e.xBias.ToString() + " "+ e.yBias.ToString() + " " + e.zBias.ToString());
        }

        private void GetPose()
        {
            if (!isConnectted)
                return;

            DobotDll.GetPose(ref pose);

            this.Dispatcher.BeginInvoke((Action)delegate ()
            {
                //if (sync.IsChecked == true)
                //{
                Xtext.Text = pose.x.ToString();
                Ytext.Text = pose.y.ToString();
                Ztext.Text = pose.z.ToString();

                J1text.Text = pose.jointAngle[0].ToString();
                J2text.Text = pose.jointAngle[1].ToString();
                J3text.Text = pose.jointAngle[2].ToString();

                sdXtext.Text = (pose.x - sd_origin.X).ToString();
                sdYtext.Text = (pose.y - sd_origin.Y).ToString();
                sdZtext.Text = (pose.z - sd_origin.Z).ToString();

                //rHead.Text = pose.rHead.ToString();
                //  pauseTime.Text = "0";
                //}
                var matrix = new Matrix3D();
               
                matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new Vector3D(0, 1, 0), pose.jointAngle[0]));
                matrix.Translate(new Vector3D((pose.y - sd_origin.Y), (pose.z - sd_origin.Z), (pose.x - sd_origin.X)));
                MeshGeometryModel3D model = ModelGroup.Children[1] as MeshGeometryModel3D;
                           
                model.Transform = new MatrixTransform3D(matrix);

              
               
            });
        }

        private void PosTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            GetPose();
        }
        private void Msg(string str, MsgInfoType infoType)
        {
            //lbTip.Content = str;
            //switch (infoType)
            //{
            //    case MsgInfoType.Error:
            //        lbTip.Foreground = new SolidColorBrush(Colors.Red);
            //        break;
            //    case MsgInfoType.Info:
            //        lbTip.Foreground = new SolidColorBrush(Colors.Black);
            //        break;
            //    default:
            //        break;
            //}
        }

        private void OnConnect_Click(object sender, RoutedEventArgs e)
        {
            StartDobot();
        }

        private void OnMoveEvent(object sender, MouseButtonEventArgs e)
        {
            if (!isConnectted)
                return;

            UInt64 cmdIndex = 0;
            Button obj = (Button)sender;
            String con = obj.Content.ToString();
            switch (con)
            {
                case "X+":
                case "Joint1+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogAPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "X-":
                case "Joint1-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogANPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Y+":
                case "Joint2+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogBPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Y-":
                case "Joint2-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogBNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Z+":
                case "Joint3+":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogCPPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                case "Z-":
                case "Joint3-":
                    {
                        currentCmd.isJoint = isJoint;
                        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogCNPressed : (byte)JogCmdType.JogIdle;
                        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    }
                    break;
                    //case "R+":
                    //case "Joint4+":
                    //    {
                    //        currentCmd.isJoint = isJoint;
                    //        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogDPPressed : (byte)JogCmdType.JogIdle;
                    //        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    //    }
                    //    break;
                    //case "R-":
                    //case "Joint4-":
                    //    {
                    //        currentCmd.isJoint = isJoint;
                    //        currentCmd.cmd = e.ButtonState == MouseButtonState.Pressed ? (byte)JogCmdType.JogDNPressed : (byte)JogCmdType.JogIdle;
                    //        DobotDll.SetJOGCmd(ref currentCmd, false, ref cmdIndex);
                    //    }
                    //    break;
                    //case "Gripper+":
                    //    {

                    //    }
                    //    break;
                    //case "Gripper-":
                    //    {

                    //    }
                    //break;
                default:
                    break;
            }
        }
        
        public void LoadModel(String FilePath)
        {

            MeshGeometryModel3D m_model = new MeshGeometryModel3D();
            //ModelGeometry已經有幾何模型存在內部 及 阻擋檔案不存在的情況
            if (IsLoaded || !File.Exists(FilePath))
            {
                return;
            }
            //利用helixtoolkit.wpf裡面提供的StlReader讀檔案，後續要轉成wpf.sharpdx可用的格式
            StLReader reader = new StLReader();

            Model3DGroup ModelContainer = reader.Read(FilePath);

            Rect3D bound = ModelContainer.Bounds;


            var ModelCenter = new Vector3(Convert.ToSingle(bound.X + bound.SizeX / 2.0),
                Convert.ToSingle(bound.Y + bound.SizeY / 2.0),
                Convert.ToSingle(bound.Z + bound.SizeZ / 2.0));

            HelixToolkit.Wpf.SharpDX.MeshGeometry3D modelGeometry = new HelixToolkit.Wpf.SharpDX.MeshGeometry3D
            {
                Normals = new Vector3Collection(),
                Positions = new Vector3Collection(),
                Indices = new IntCollection()
            };




            foreach (System.Windows.Media.Media3D.Model3D model in ModelContainer.Children)
            {
                var geometryModel = model as System.Windows.Media.Media3D.GeometryModel3D;
              
                System.Windows.Media.Media3D.MeshGeometry3D mesh = geometryModel?.Geometry as System.Windows.Media.Media3D.MeshGeometry3D;

                if (mesh == null)
                    continue;

                //將從stlreader讀到的資料轉入
                foreach (Point3D position in mesh.Positions)
                {
                    modelGeometry.Positions.Add(new Vector3(
                        Convert.ToSingle(position.X)
                        , Convert.ToSingle(position.Y)
                        , Convert.ToSingle(position.Z)));
                }
                foreach (Vector3D normal in mesh.Normals)
                {
                    modelGeometry.Normals.Add(new Vector3(
                        Convert.ToSingle(normal.X)
                        , Convert.ToSingle(normal.Y)
                        , Convert.ToSingle(normal.Z)));
                }
                foreach (Int32 triangleindice in mesh.TriangleIndices)
                {
                    modelGeometry.Indices.Add(triangleindice);
                }
            }

          

            SetModelMaterial(m_model);
            
            m_model.Geometry = modelGeometry;

            

            ModelGroup.Children.Add(m_model);

            m_group.Add(m_model);

            ResetCameraPosition(bound);

           
        }
        public void SetModelMaterial(MeshGeometryModel3D model)
        {

            PhongMaterial material = new PhongMaterial
            {
                ReflectiveColor = SharpDX.Color.Black,
                AmbientColor = new SharpDX.Color(0.0f, 0.0f, 0.0f, 1.0f),
                EmissiveColor = SharpDX.Color.Black,
                SpecularColor = new SharpDX.Color(90, 90, 90, 255),
                SpecularShininess = 60,
                DiffuseColor = SharpDX.Color.Blue
            };
           
            model.Material = material;
        }
        public static HelixToolkit.Wpf.SharpDX.Camera Camera1
        {
            get;
        } = new HelixToolkit.Wpf.SharpDX.OrthographicCamera();
    
        public void ResetCameraPosition(Rect3D boundingBox)
        {
            //var boneCollection = MainViewModel.ProjData.BoneCollection;
            //

             Point3D modelCenter = new Point3D(boundingBox.X + boundingBox.SizeX / 2.0, boundingBox.Y + boundingBox.SizeY / 2.0, boundingBox.Z + boundingBox.SizeZ / 2.0);
            //Point3D modelCenter = new Point3D(0,200,0);
         
            HelixToolkit.Wpf.SharpDX.OrthographicCamera orthoCam1 = Camera1 as HelixToolkit.Wpf.SharpDX.OrthographicCamera;
            if (orthoCam1 != null)
            {
                orthoCam1.Position = new Point3D(modelCenter.X, modelCenter.Y, modelCenter.Z + 1000*boundingBox.Size.Z);
                orthoCam1.UpDirection = new Vector3D(0, 1, 0);
                orthoCam1.LookDirection = new Vector3D(0, 0, -1000 * boundingBox.Size.Z);
                orthoCam1.NearPlaneDistance = 0;
                orthoCam1.FarPlaneDistance = 1e15;
                orthoCam1.Width =300;
             }
           
            hVp3D.Camera = orthoCam1;

            HelixToolkit.Wpf.SharpDX.AmbientLight3D Light1 = new HelixToolkit.Wpf.SharpDX.AmbientLight3D();
            HelixToolkit.Wpf.SharpDX.DirectionalLight3D Light2 = new HelixToolkit.Wpf.SharpDX.DirectionalLight3D();
            HelixToolkit.Wpf.SharpDX.DirectionalLight3D Light3 = new HelixToolkit.Wpf.SharpDX.DirectionalLight3D();
            Light1.Color = new SharpDX.Color(1.0f, 1.0f, 1.0f);
            Light2.Color = new SharpDX.Color(0.2f, 0.2f, 0.2f);
            Light3.Color = new SharpDX.Color(0.5f, 0.5f, 0.5f);
            Light1.Direction = new Vector3(-10, -10, -(float)boundingBox.Size.Z);
            Light2.Direction = new Vector3(-10, 0, -(float)boundingBox.Size.Z);
            Light3.Direction = new Vector3(-1000, -(float)boundingBox.Size.Y, -30);
            hVp3D.Items.Add(Light1);

            hVp3D.Items.Add(Light2);
            hVp3D.Items.Add(Light3);

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            DobotDll.ResetPose(false, 0, 0);
            DobotDll.GetPose(ref pose);
            sd_origin = new Point3D(pose.x, pose.y, pose.z);        
        }

        //public void load3dModel()
        //{
        //    //ObjReader CurrentHelixObjReader = new ObjReader();
        //    //StLReader CurrentHelixStlReader = new StLReader();
        //    // Model3DGroup MyModel = CurrentHelixObjReader.Read(@"D:\3DModel\dinosaur_FBX\dinosaur.fbx");
        //    Model3D MyModel = CurrentHelixStlReader.Read(@"E:\邱灃毅_LPlaster.stl");
        //    ModelVisual3D model = new ModelVisual3D();
        //    model.Content = MyModel;


        //  //  hVp3D.Children.Add(teaPot);

        //   // hVp3D.Children.Add(model);
        //    //model.Content = MyModel;
        //    //MyModel.Children.Add(MyModel);


        //}
    }
}
