using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;
using ApxLabs.FastAndroidCamera;

namespace ZXing.Mobile.CameraAccess
{
    public class CameraAnalyzer
    {
        private readonly CameraController _cameraController;
        private readonly CameraEventsListener _cameraEventListener;
        public EventHandler<byte[]> OnPictureTaken;
        private Task _processingTask;
        private DateTime _lastPreviewAnalysis = DateTime.UtcNow;
        private bool _wasScanned;
        IScannerSessionHost _scannerHost;

        public CameraAnalyzer(SurfaceView surfaceView, IScannerSessionHost scannerHost)
        {
            _scannerHost = scannerHost;
            _cameraEventListener = new CameraEventsListener();
            _cameraController = new CameraController(surfaceView, _cameraEventListener, scannerHost);
            Torch = new Torch(_cameraController, surfaceView.Context);
        }

        public event EventHandler<Result> BarcodeFound;

        public Torch Torch { get; }

        public bool IsAnalyzing { get; private set; }

        public void PauseAnalysis()
        {
            IsAnalyzing = false;
        }

        public void ResumeAnalysis()
        {
            IsAnalyzing = true;
        }

        public void ShutdownCamera()
        {
            IsAnalyzing = false;
            _cameraEventListener.OnPreviewFrameReady -= HandleOnPreviewFrameReady;
            _cameraController.ShutdownCamera();
        }

        public void SetupCamera()
        {
            _cameraEventListener.OnPreviewFrameReady += HandleOnPreviewFrameReady;
            _cameraController.SetupCamera();
        }

        public void AutoFocus()
        {
            _cameraController.AutoFocus();
        }

        public void AutoFocus(int x, int y)
        {
            _cameraController.AutoFocus(x, y);
        }

        public void RefreshCamera()
        {
            _cameraController.RefreshCamera();
        }

        private bool CanAnalyzeFrame
        {
            get
            {
                if (!IsAnalyzing)
                    return false;

                //Check and see if we're still processing a previous frame
                // todo: check if we can run as many as possible or mby run two analyzers at once (Vision + ZXing)
                if (_processingTask != null && !_processingTask.IsCompleted)
                    return false;

                var elapsedTimeMs = (DateTime.UtcNow - _lastPreviewAnalysis).TotalMilliseconds;
                if (elapsedTimeMs < _scannerHost.ScanningOptions.DelayBetweenAnalyzingFrames)
                    return false;

                // Delay a minimum between scans
                if (_wasScanned && elapsedTimeMs < _scannerHost.ScanningOptions.DelayBetweenContinuousScans)
                    return false;

                return true;
            }
        }

        private void HandleOnPreviewFrameReady(object sender, FastJavaByteArray fastArray)
        {

            if (!CanAnalyzeFrame)
                return;

            _wasScanned = false;
            _lastPreviewAnalysis = DateTime.UtcNow;

            _processingTask = Task.Run(() =>
            {
                try
                {
                    if (TakePicture)
                    {

                        TakePicture = false;
                        var raw = new byte[fastArray.Count];
                        fastArray.CopyTo(raw, 0);
                        var img = new YuvImage(raw, ImageFormatType.Nv21, _cameraController.Camera.GetParameters().PreviewSize.Width, _cameraController.Camera.GetParameters().PreviewSize.Height, null);
                        using (var stream = new MemoryStream())
                        {
                            var rect = new Rect(0, 0, img.Width, img.Height);
                            img.CompressToJpeg(rect, 80, stream);
                            var array = stream.ToArray();
                            OnPictureTaken?.Invoke(this, array);
                            stream.Close();
                        }
                    }


                    DecodeFrame(fastArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Android.Util.Log.Debug(MobileBarcodeScanner.TAG, "DecodeFrame exception occurs");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public bool TakePicture { get; set; } = false;

        private void DecodeFrame(FastJavaByteArray fastArray)
        {
            var cameraParameters = _cameraController.Camera.GetParameters();
            var width = cameraParameters.PreviewSize.Width;
            var height = cameraParameters.PreviewSize.Height;

            var barcodeReader = _scannerHost.ScanningOptions.BuildBarcodeReader();

            var rotate = false;
            var newWidth = width;
            var newHeight = height;

            // use last value for performance gain
            var cDegrees = _cameraController.LastCameraDisplayOrientationDegree;

            if (cDegrees == 90 || cDegrees == 270)
            {
                rotate = true;
                newWidth = height;
                newHeight = width;
            }

            ZXing.Result result = null;
            var start = PerformanceCounter.Start();

            LuminanceSource fast = new FastJavaByteArrayYUVLuminanceSource(fastArray, width, height, 0, 0, width, height); // _area.Left, _area.Top, _area.Width, _area.Height);
            if (rotate)
                fast = fast.rotateCounterClockwise();

            result = barcodeReader.Decode(fast);

            fastArray.Dispose();
            fastArray = null;

            PerformanceCounter.Stop(start,
                "Decode Time: {0} ms (width: " + width + ", height: " + height + ", degrees: " + cDegrees + ", rotate: " +
                rotate + ")");

            if (result != null)
            {
                Android.Util.Log.Debug(MobileBarcodeScanner.TAG, "Barcode Found");

                _wasScanned = true;
                BarcodeFound?.Invoke(this, result);
                return;
            }
        }
    }
}