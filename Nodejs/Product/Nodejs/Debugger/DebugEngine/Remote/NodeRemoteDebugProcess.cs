﻿//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote {
    internal class NodeRemoteDebugProcess : IDebugProcess2, IDebugProcessSecurity2 {
        // Remote processes always have this ID.
        public const int RemoteId = 1;

        private readonly NodeRemoteDebugPort _port;
        private readonly int _id;
        private readonly string _exe;
        private readonly string _username;
        private readonly string _version;
        private readonly Guid _guid = Guid.NewGuid();
        private NodeRemoteEnumDebugPrograms _programs;

        public NodeRemoteDebugProcess(NodeRemoteDebugPort port, string exe, string username, string version) {
            _port = port;
            _id = RemoteId;
            _username = username;
            _exe = string.IsNullOrEmpty(exe) ? "<node>" : exe;
            _version = version;
        }

        public NodeRemoteDebugPort DebugPort {
            get { return _port; }
        }

        public int Id {
            get { return _id; }
        }

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach) {
            throw new NotImplementedException();
        }

        public int CanDetach() {
            return VSConstants.S_OK;
        }

        public int CauseBreak() {
            throw new NotImplementedException();
        }

        public int Detach() {
            return VSConstants.S_OK;
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum) {
            if (_programs != null) {
                return _programs.Clone(out ppEnum);
            }
            _programs = new NodeRemoteEnumDebugPrograms(this);
            ppEnum = _programs;
            return VSConstants.S_OK;
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum) {
            throw new NotImplementedException();
        }

        public int GetAttachedSessionName(out string pbstrSessionName) {
            throw new NotImplementedException();
        }

        public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo) {
            // The various string fields should match the strings returned by GetName - keep them in sync when making any changes here.
            var pi = new PROCESS_INFO();
            pi.Fields = Fields;
            pi.bstrFileName = _exe;
            pi.bstrBaseName = BaseName;
            pi.bstrTitle = Title;
            pi.ProcessId.dwProcessId = (uint)_id;
            pProcessInfo[0] = pi;
            return VSConstants.S_OK;
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName) {
            // The return value should match the corresponding string field returned from GetInfo - keep them in sync when making any changes here.
            switch (gnType) {
                case enum_GETNAME_TYPE.GN_FILENAME:
                    pbstrName = _exe;
                    break;
                case enum_GETNAME_TYPE.GN_BASENAME:
                    pbstrName = BaseName;
                    break;
                case enum_GETNAME_TYPE.GN_NAME:
                case enum_GETNAME_TYPE.GN_TITLE:
                    pbstrName = _version;
                    break;
                default:
                    pbstrName = null;
                    break;
            }
            return VSConstants.S_OK;
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId) {
            var pidStruct = new AD_PROCESS_ID();
            pidStruct.dwProcessId = (uint)_id;
            pProcessId[0] = pidStruct;
            return VSConstants.S_OK;
        }

        public int GetPort(out IDebugPort2 ppPort) {
            ppPort = _port;
            return VSConstants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId) {
            pguidProcessId = _guid;
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer) {
            throw new NotImplementedException();
        }

        public int Terminate() {
            throw new NotImplementedException();
        }

        public int GetUserName(out string pbstrUserName) {
            pbstrUserName = _username;
            return VSConstants.S_OK;
        }

        public int QueryCanSafelyAttach() {
            return VSConstants.S_OK;
        }

        private string BaseName {
            get {
                string portName;
                _port.GetPortName(out portName);
                return Path.GetFileName(_exe) + " @ " + portName;
            }
        }

        private string Title {
            get { return _version; }
        }
    }
}
