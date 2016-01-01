#if WINDOWS_UWP
using System;

namespace AudioUtilitiesUWP
{
    class Audio
    {
        private Windows.Media.Audio.AudioGraph graph;
        private Windows.Media.Audio.AudioDeviceOutputNode deviceOutputNode; // usually speaker
        private Windows.Media.Audio.AudioFileOutputNode fileOutputNode;
        private Windows.Media.Audio.AudioDeviceInputNode deviceInputNode; // usually microphone

        public void Start()
        {
            if (graph != null)
                graph.Start(); // starts all devices on graph
        }

        public async void Stop()
        {
            if (graph != null)
            {
                graph.Stop(); // stops all devices on graph
                if (fileOutputNode != null)
                {
                    // required to finalize writing to file
                    Windows.Media.Transcoding.TranscodeFailureReason finalizeResult = await fileOutputNode.FinalizeAsync();
                    if (finalizeResult != Windows.Media.Transcoding.TranscodeFailureReason.None)
                    {
                        WoundifyShared.Log.WriteLine("Stop: TranscodeFailureReason:" + finalizeResult.ToString());
                        return;
                    }
                }
            }
        }

        public void Dispose() // todo: implement IDispose
        {
            if (graph != null)
            {
                graph.Dispose();
                graph = null;
            }
        }

        public async System.Threading.Tasks.Task CreateAudioGraphAsync(string outputFileName)
        {
            Windows.Media.Audio.AudioGraphSettings graphSettings = new Windows.Media.Audio.AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);
            graphSettings.QuantumSizeSelectionMode = Windows.Media.Audio.QuantumSizeSelectionMode.LowestLatency;

            // todo: let user pick from list of devices instead of blindly picking first one -- hoping for the microphone.
            Windows.Devices.Enumeration.DeviceInformationCollection deviceInformationCollection = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.AudioRender);
            Windows.Devices.Enumeration.DeviceInformation deviceInformation = deviceInformationCollection[0]; // blindly pick first one
            graphSettings.PrimaryRenderDevice = deviceInformation;

            Windows.Media.Audio.CreateAudioGraphResult result = await Windows.Media.Audio.AudioGraph.CreateAsync(graphSettings);

            if (result.Status != Windows.Media.Audio.AudioGraphCreationStatus.Success)
            {
                WoundifyShared.Log.WriteLine("Cannot create graph:" + result.Status.ToString());
                return;
            }

            graph = result.Graph;

            await AudioDeviceInputAsync(); // setup microphone (usually)
            //await AudioDeviceOutputAsync(); // setup speakers (usually)
            await AudioFileOutputAsync(WoundifyShared.Options.options.tempFolderPath + outputFileName); // setup file

            deviceInputNode.AddOutgoingConnection(fileOutputNode);
        }

        private async System.Threading.Tasks.Task AudioDeviceInputAsync()
        {
            // Create a device input node (usually microphone)
            Windows.Media.Audio.CreateAudioDeviceInputNodeResult deviceInputNodeResult = await graph.CreateDeviceInputNodeAsync(Windows.Media.Capture.MediaCategory.Other);
            if (deviceInputNodeResult.Status != Windows.Media.Audio.AudioDeviceNodeCreationStatus.Success)
            {
                WoundifyShared.Log.WriteLine("Audio device input unavailable:" + deviceInputNodeResult.Status.ToString());
                return;
            }

            deviceInputNode = deviceInputNodeResult.DeviceInputNode;
        }

        private async System.Threading.Tasks.Task AudioDeviceOutputAsync()
        {
            // Create a device output node (usually speakers)
            Windows.Media.Audio.CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != Windows.Media.Audio.AudioDeviceNodeCreationStatus.Success)
            {
                WoundifyShared.Log.WriteLine("Audio device output unavailable:" + deviceOutputNodeResult.Status.ToString());
                return;
            }

            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

            deviceInputNode.AddOutgoingConnection(deviceOutputNode); // add this node to the input's list of outputs
        }

        private async System.Threading.Tasks.Task AudioFileOutputAsync(string outputFileName)
        {
            Windows.Media.MediaProperties.MediaEncodingProfile mediaEncodingProfile = new Windows.Media.MediaProperties.MediaEncodingProfile();
            mediaEncodingProfile = Windows.Media.MediaProperties.MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Low); // Low defaults to mono - good
            // hmmmm, suspect that any attempt to change ChannelCount really doesn't do anything (AudioEncodingQuality.High)
            mediaEncodingProfile.Audio.ChannelCount = 1; // careful - stereo not permitted for some speech applications so must use mono
            Windows.Storage.StorageFolder tempFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(WoundifyShared.Options.options.tempFolderPath);
            Windows.Storage.StorageFile srcFile = await tempFolder.CreateFileAsync(WoundifyShared.Options.options.audio.speechSynthesisFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Windows.Storage.StorageFile file = await tempFolder.CreateFileAsync(outputFileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Windows.Media.Audio.CreateAudioFileOutputNodeResult fileOutputNodeResult = await graph.CreateFileOutputNodeAsync(file, mediaEncodingProfile);
            if (fileOutputNodeResult.Status != Windows.Media.Audio.AudioFileNodeCreationStatus.Success)
            {
                WoundifyShared.Log.WriteLine("Audio device output unavailable:" + fileOutputNodeResult.Status.ToString());
                return;
            }

            fileOutputNode = fileOutputNodeResult.FileOutputNode;
        }
    }
}
#endif
