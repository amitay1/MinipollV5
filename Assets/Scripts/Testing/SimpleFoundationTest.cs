using UnityEngine;
using Minipoll.Core.Events;
using Minipoll.Core.Events.Types;
using Minipoll.Core.Architecture;
using Minipoll.Core.Management;

namespace Minipoll.Testing
{
    /// <summary>
    /// Simple test script to verify Foundation Systems work without Unity MCP connection
    /// This script can be manually added to a GameObject in the Unity Editor
    /// </summary>
    public class SimpleFoundationTest : MonoBehaviour
    {
        [Header("Test Settings")]
        public bool runTestOnStart = true;
        public bool logDetailedInfo = true;
        
        [Header("Test Results")]
        public bool eventSystemWorking = false;
        public bool prefabArchitectureWorking = false;
        public bool systemManagerWorking = false;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                Debug.Log("🚀 [SimpleFoundationTest] Starting Foundation Systems Test");
                TestFoundationSystems();
            }
        }
        
        public void TestFoundationSystems()
        {
            // Test 1: Event System
            TestEventSystem();
            
            // Test 2: Prefab Architecture
            TestPrefabArchitecture();
            
            // Test 3: System Manager
            TestSystemManager();
            
            // Summary
            int passedTests = 0;
            if (eventSystemWorking) passedTests++;
            if (prefabArchitectureWorking) passedTests++;
            if (systemManagerWorking) passedTests++;
            
            Debug.Log($"📊 [SimpleFoundationTest] Test Results: {passedTests}/3 systems working");
            
            if (passedTests == 3)
            {
                Debug.Log("✅ [SimpleFoundationTest] ALL FOUNDATION SYSTEMS WORKING!");
            }
            else
            {
                Debug.LogWarning($"⚠️ [SimpleFoundationTest] {3 - passedTests} systems need attention");
            }
        }
        
        private void TestEventSystem()
        {
            try
            {
                Debug.Log("🧪 [SimpleFoundationTest] Testing Event System...");
                
                bool eventReceived = false;
                
                // Subscribe to a test event
                System.Action<SystemInitializedEvent> handler = (eventData) => {
                    eventReceived = true;
                    if (logDetailedInfo)
                        Debug.Log($"✅ [SimpleFoundationTest] Received event: {eventData.SystemName}");
                };
                
                EventSystem.Subscribe<SystemInitializedEvent>(handler);
                
                // Publish a test event
                EventSystem.Publish(new SystemInitializedEvent 
                {
                    SystemName = "TestSystem",
                    Success = true,
                    Message = "Event System Test"
                });
                
                // Check if event was received
                if (eventReceived)
                {
                    eventSystemWorking = true;
                    Debug.Log("✅ [SimpleFoundationTest] Event System: WORKING");
                }
                else
                {
                    eventSystemWorking = false;
                    Debug.LogError("❌ [SimpleFoundationTest] Event System: FAILED");
                }
                
                // Clean up
                EventSystem.Unsubscribe<SystemInitializedEvent>(handler);
            }
            catch (System.Exception ex)
            {
                eventSystemWorking = false;
                Debug.LogError($"❌ [SimpleFoundationTest] Event System Exception: {ex.Message}");
            }
        }
        
        private void TestPrefabArchitecture()
        {
            try
            {
                Debug.Log("🧪 [SimpleFoundationTest] Testing Prefab Architecture...");
                
                var prefabArch = MinipollPrefabArchitecture.Instance;
                
                if (prefabArch != null)
                {
                    prefabArchitectureWorking = true;
                    Debug.Log("✅ [SimpleFoundationTest] Prefab Architecture: WORKING");
                    
                    if (logDetailedInfo)
                    {
                        Debug.Log($"📊 [SimpleFoundationTest] Prefab Architecture Debug Info:\n{prefabArch.GetDebugInfo()}");
                    }
                }
                else
                {
                    prefabArchitectureWorking = false;
                    Debug.LogError("❌ [SimpleFoundationTest] Prefab Architecture: Instance is null");
                }
            }
            catch (System.Exception ex)
            {
                prefabArchitectureWorking = false;
                Debug.LogError($"❌ [SimpleFoundationTest] Prefab Architecture Exception: {ex.Message}");
            }
        }
        
        private void TestSystemManager()
        {
            try
            {
                Debug.Log("🧪 [SimpleFoundationTest] Testing System Manager...");
                
                var systemManager = SystemManager.Instance;
                
                if (systemManager != null)
                {
                    systemManagerWorking = true;
                    Debug.Log("✅ [SimpleFoundationTest] System Manager: WORKING");
                    
                    if (logDetailedInfo)
                    {
                        Debug.Log($"📊 [SimpleFoundationTest] System Manager Debug Info:\n{systemManager.GetDebugInfo()}");
                    }
                }
                else
                {
                    systemManagerWorking = false;
                    Debug.LogError("❌ [SimpleFoundationTest] System Manager: Instance is null");
                }
            }
            catch (System.Exception ex)
            {
                systemManagerWorking = false;
                Debug.LogError($"❌ [SimpleFoundationTest] System Manager Exception: {ex.Message}");
            }
        }
        
        // Context menu items for testing in editor
        [ContextMenu("Test Foundation Systems")]
        public void TestFromContextMenu()
        {
            TestFoundationSystems();
        }
        
        [ContextMenu("Test Event System Only")]
        public void TestEventSystemOnly()
        {
            TestEventSystem();
        }
        
        [ContextMenu("Test Prefab Architecture Only")]
        public void TestPrefabArchitectureOnly()
        {
            TestPrefabArchitecture();
        }
        
        [ContextMenu("Test System Manager Only")]
        public void TestSystemManagerOnly()
        {
            TestSystemManager();
        }
    }
}
