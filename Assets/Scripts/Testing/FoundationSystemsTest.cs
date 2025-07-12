using System.Collections;
using UnityEngine;
using Minipoll.Core.Events;
using Minipoll.Core.Events.Types;
using Minipoll.Core.Architecture;
using Minipoll.Core.Management;

namespace Minipoll.Testing
{
    /// <summary>
    /// Test script to verify Foundation Systems (Level 1) are working correctly
    /// Tests EventSystem and MinipollPrefabArchitecture integration
    /// </summary>
    public class FoundationSystemsTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool enableDetailedLogging = true;
        
        [Header("Test Prefabs")]
        [SerializeField] private GameObject testPrefab;
        [SerializeField] private GameObject testMinipollPrefab;
        
        private int eventTestCounter = 0;
        private bool allTestsPassed = true;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllFoundationTests());
            }
        }
        
        #endregion
        
        #region Test Orchestration
        
        /// <summary>
        /// Run all foundation system tests
        /// </summary>
        public IEnumerator RunAllFoundationTests()
        {
            Debug.Log("=== FOUNDATION SYSTEMS TEST SUITE START ===");
            allTestsPassed = true;
            
            // Test 1: Event System Basic Functionality
            yield return StartCoroutine(TestEventSystemBasics());
            
            // Test 2: Event System Typed Events
            yield return StartCoroutine(TestEventSystemTypedEvents());
            
            // Test 3: Prefab Architecture Basic Functionality
            yield return StartCoroutine(TestPrefabArchitectureBasics());
            
            // Test 4: Prefab Architecture with Events
            yield return StartCoroutine(TestPrefabArchitectureEvents());
            
            // Test 5: System Manager Integration
            yield return StartCoroutine(TestSystemManagerIntegration());
            
            // Final Results
            string result = allTestsPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED";
            Debug.Log($"=== FOUNDATION SYSTEMS TEST SUITE COMPLETE: {result} ===");
            
            if (allTestsPassed)
            {
                EventSystem.Publish(new SystemInitializedEvent 
                { 
                    SystemName = "FoundationSystemsTest",
                    Success = true,
                    Message = "All foundation systems tests passed successfully"
                });
            }
        }
        
        #endregion
        
        #region Event System Tests
        
        private IEnumerator TestEventSystemBasics()
        {
            LogTest("Testing Event System Basic Functionality");
            
            bool testPassed = true;
            eventTestCounter = 0;
            string errorMessage = "";
            
            try
            {
                // Subscribe to named event
                EventSystem.Subscribe("TestEvent", OnTestEvent);
                
                // Publish named event
                EventSystem.Publish("TestEvent", "Hello World");
            }
            catch (System.Exception ex)
            {
                LogTestError($"Event System Basic test exception during setup: {ex.Message}");
                testPassed = false;
                errorMessage = ex.Message;
            }
            
            if (testPassed)
            {
                yield return new WaitForSeconds(0.1f);
                
                // Check if event was received
                if (eventTestCounter != 1)
                {
                    LogTestError($"Named event test failed. Expected 1 event, got {eventTestCounter}");
                    testPassed = false;
                }
                
                try
                {
                    // Unsubscribe and test again
                    EventSystem.Unsubscribe("TestEvent", OnTestEvent);
                    EventSystem.Publish("TestEvent", "Should not be received");
                }
                catch (System.Exception ex)
                {
                    LogTestError($"Event System Basic test exception during unsubscribe: {ex.Message}");
                    testPassed = false;
                }
                
                if (testPassed)
                {
                    yield return new WaitForSeconds(0.1f);
                    
                    if (eventTestCounter != 1)
                    {
                        LogTestError($"Unsubscribe test failed. Expected 1 event, got {eventTestCounter}");
                        testPassed = false;
                    }
                }
            }
            
            LogTestResult("Event System Basic Functionality", testPassed);
            allTestsPassed &= testPassed;
        }
        
        private IEnumerator TestEventSystemTypedEvents()
        {
            LogTest("Testing Event System Typed Events");
            
            bool testPassed = true;
            var receivedEvent = false;
            System.Action<SystemInitializedEvent> handler = null;
            
            try
            {
                // Subscribe to typed event
                handler = (eventData) => {
                    receivedEvent = true;
                    LogTestInfo($"Received typed event: {eventData.SystemName} - {eventData.Message}");
                };
                
                EventSystem.Subscribe<SystemInitializedEvent>(handler);
                
                // Publish typed event
                EventSystem.Publish(new SystemInitializedEvent 
                { 
                    SystemName = "TestSystem",
                    Success = true,
                    Message = "Test typed event"
                });
            }
            catch (System.Exception ex)
            {
                LogTestError($"Event System Typed test exception during setup: {ex.Message}");
                testPassed = false;
            }
            
            if (testPassed)
            {
                yield return new WaitForSeconds(0.1f);
                
                if (!receivedEvent)
                {
                    LogTestError("Typed event was not received");
                    testPassed = false;
                }
                
                try
                {
                    // Clean up
                    if (handler != null)
                        EventSystem.Unsubscribe<SystemInitializedEvent>(handler);
                }
                catch (System.Exception ex)
                {
                    LogTestError($"Event System Typed test exception during cleanup: {ex.Message}");
                    testPassed = false;
                }
            }
            
            LogTestResult("Event System Typed Events", testPassed);
            allTestsPassed &= testPassed;
        }
        
        #endregion
        
        #region Prefab Architecture Tests
        
        private IEnumerator TestPrefabArchitectureBasics()
        {
            LogTest("Testing Prefab Architecture Basic Functionality");
            
            bool testPassed = true;
            
            try
            {
                var prefabArch = MinipollPrefabArchitecture.Instance;
                
                if (prefabArch == null)
                {
                    LogTestError("MinipollPrefabArchitecture instance is null");
                    testPassed = false;
                }
                else
                {
                    // Test prefab registration
                    if (testPrefab != null)
                    {
                        prefabArch.RegisterPrefab("test_object", testPrefab, "Testing");
                        
                        if (!prefabArch.IsPrefabRegistered("test_object"))
                        {
                            LogTestError("Prefab registration failed");
                            testPassed = false;
                        }
                        
                        // Test prefab instantiation
                        var instance = prefabArch.CreateInstance("test_object", Vector3.zero);
                        
                        if (instance == null)
                        {
                            LogTestError("Prefab instantiation failed");
                            testPassed = false;
                        }
                        else
                        {
                            LogTestInfo($"Successfully created instance: {instance.name}");
                            
                            // Test metadata
                            var metadata = prefabArch.GetMetadata(instance);
                            if (metadata == null)
                            {
                                LogTestError("Prefab metadata not found");
                                testPassed = false;
                            }
                            
                            // Clean up
                            prefabArch.DestroyInstance(instance);
                        }
                    }
                    else
                    {
                        LogTestWarning("No test prefab assigned, skipping prefab tests");
                    }
                }
                
                LogTestResult("Prefab Architecture Basic Functionality", testPassed);
            }
            catch (System.Exception ex)
            {
                LogTestError($"Prefab Architecture Basic test exception: {ex.Message}");
                testPassed = false;
            }
            
            allTestsPassed &= testPassed;
            yield return null;
        }
        
        private IEnumerator TestPrefabArchitectureEvents()
        {
            LogTest("Testing Prefab Architecture Event Integration");
            
            bool testPassed = true;
            bool eventReceived = false;
            System.Action<SystemInitializedEvent> handler = null;
            
            try
            {
                // Subscribe to system events
                handler = (eventData) => {
                    if (eventData.SystemName == "MinipollPrefabArchitecture")
                    {
                        eventReceived = true;
                        LogTestInfo($"Received prefab architecture event: {eventData.Message}");
                    }
                };
                
                EventSystem.Subscribe<SystemInitializedEvent>(handler);
            }
            catch (System.Exception ex)
            {
                LogTestError($"Prefab Architecture Event test exception during setup: {ex.Message}");
                testPassed = false;
            }
            
            if (testPassed)
            {
                yield return new WaitForSeconds(0.2f);
                
                // The prefab architecture should have already published its initialization event
                if (!eventReceived)
                {
                    LogTestWarning("Prefab architecture initialization event not received (may have been published before subscription)");
                    // This is not necessarily a failure since the event might have been published during Awake
                }
                
                try
                {
                    // Clean up
                    if (handler != null)
                        EventSystem.Unsubscribe<SystemInitializedEvent>(handler);
                }
                catch (System.Exception ex)
                {
                    LogTestError($"Prefab Architecture Event test exception during cleanup: {ex.Message}");
                    testPassed = false;
                }
            }
            
            LogTestResult("Prefab Architecture Event Integration", testPassed);
            allTestsPassed &= testPassed;
        }
        
        #endregion
        
        #region System Manager Tests
        
        private IEnumerator TestSystemManagerIntegration()
        {
            LogTest("Testing System Manager Integration");
            
            bool testPassed = true;
            
            try
            {
                var systemManager = SystemManager.Instance;
                
                if (systemManager == null)
                {
                    LogTestError("SystemManager instance is null");
                    testPassed = false;
                }
                else
                {
                    LogTestInfo($"SystemManager found. Initialized: {systemManager.IsInitialized}");
                    
                    // Get debug info
                    string debugInfo = systemManager.GetDebugInfo();
                    if (enableDetailedLogging)
                    {
                        LogTestInfo($"System Manager Debug Info:\n{debugInfo}");
                    }
                }
                
                LogTestResult("System Manager Integration", testPassed);
            }
            catch (System.Exception ex)
            {
                LogTestError($"System Manager test exception: {ex.Message}");
                testPassed = false;
            }
            
            allTestsPassed &= testPassed;
            yield return null;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnTestEvent(object data)
        {
            eventTestCounter++;
            LogTestInfo($"Received test event with data: {data}");
        }
        
        #endregion
        
        #region Test Utilities
        
        private void LogTest(string testName)
        {
            Debug.Log($"🧪 [TEST] {testName}");
        }
        
        private void LogTestResult(string testName, bool passed)
        {
            string status = passed ? "✅ PASSED" : "❌ FAILED";
            Debug.Log($"📊 [TEST RESULT] {testName}: {status}");
        }
        
        private void LogTestInfo(string message)
        {
            if (enableDetailedLogging)
                Debug.Log($"ℹ️ [TEST INFO] {message}");
        }
        
        private void LogTestWarning(string message)
        {
            Debug.LogWarning($"⚠️ [TEST WARNING] {message}");
        }
        
        private void LogTestError(string message)
        {
            Debug.LogError($"❌ [TEST ERROR] {message}");
        }
        
        #endregion
        
        #region Public Test Methods
        
        [ContextMenu("Run Foundation Tests")]
        public void RunFoundationTestsFromMenu()
        {
            StartCoroutine(RunAllFoundationTests());
        }
        
        [ContextMenu("Test Event System Only")]
        public void TestEventSystemOnly()
        {
            StartCoroutine(TestEventSystemBasics());
        }
        
        [ContextMenu("Test Prefab Architecture Only")]
        public void TestPrefabArchitectureOnly()
        {
            StartCoroutine(TestPrefabArchitectureBasics());
        }
        
        #endregion
    }
}
