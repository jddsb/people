mergeInto(LibraryManager.library, {
    StarkSidebarGetLaunchOptions: function(callbackId) {
        try {
            var launchOptions = null;
            if (typeof tt !== 'undefined' && tt.getLaunchOptionsSync) {
                launchOptions = tt.getLaunchOptionsSync();
            }
            
            var result = JSON.stringify(launchOptions || {});
            var bufferSize = lengthBytesUTF8(result) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(result, buffer, bufferSize);
            
            if (typeof window.StarkSDK !== 'undefined' && window.StarkSDK.SendMessage) {
                window.StarkSDK.SendMessage(callbackId, buffer);
            }
            
            return buffer;
        } catch (e) {
            console.error("StarkSidebarGetLaunchOptions error:", e);
            return _malloc(2);
        }
    },
    
    StarkSidebarReportEntry: function() {
        try {
            if (typeof tt !== 'undefined' && tt.reportSideBarEntry) {
                tt.reportSideBarEntry({
                    success: function(res) {
                        console.log("Sidebar entry reported:", res);
                    },
                    fail: function(err) {
                        console.error("Failed to report sidebar entry:", err);
                    }
                });
            }
        } catch (e) {
            console.error("StarkSidebarReportEntry error:", e);
        }
    },
    
    StarkSidebarNavigateToScene: function(scene) {
        try {
            var sceneStr = _StarkPointerStringify(scene);
            if (typeof tt !== 'undefined' && tt.navigateToScene) {
                tt.navigateToScene({
                    scene: sceneStr,
                    success: function(res) {
                        console.log("Navigate to scene success:", res);
                    },
                    fail: function(err) {
                        console.error("Navigate to scene failed:", err);
                    }
                });
            }
        } catch (e) {
            console.error("StarkSidebarNavigateToScene error:", e);
        }
    },
    
    StarkSidebarRegisterRetain: function(data) {
        try {
            var dataStr = _StarkPointerStringify(data);
            var dataObj = JSON.parse(dataStr || '{}');
            
            if (typeof tt !== 'undefined' && tt.registerRetainSideBar) {
                tt.registerRetainSideBar({
                    scene: 'sidebar',
                    data: dataObj,
                    success: function(res) {
                        console.log("Register retain sidebar success:", res);
                    },
                    fail: function(err) {
                        console.error("Register retain sidebar failed:", err);
                    }
                });
            }
        } catch (e) {
            console.error("StarkSidebarRegisterRetain error:", e);
        }
    },
    
    StarkSidebarEnterRetain: function() {
        try {
            if (typeof tt !== 'undefined' && tt.enterRetainSideBar) {
                tt.enterRetainSideBar({
                    success: function(res) {
                        console.log("Enter retain sidebar success:", res);
                    },
                    fail: function(err) {
                        console.error("Enter retain sidebar failed:", err);
                    }
                });
            }
        } catch (e) {
            console.error("StarkSidebarEnterRetain error:", e);
        }
    },
    
    StarkSidebarReportTask: function(taskId, data) {
        try {
            var taskIdStr = _StarkPointerStringify(taskId);
            var dataStr = _StarkPointerStringify(data);
            var dataObj = JSON.parse(dataStr || '{}');
            
            if (typeof tt !== 'undefined' && tt.reportSideBarTask) {
                tt.reportSideBarTask({
                    taskId: taskIdStr,
                    data: dataObj,
                    success: function(res) {
                        console.log("Report task success:", res);
                    },
                    fail: function(err) {
                        console.error("Report task failed:", err);
                    }
                });
            }
        } catch (e) {
            console.error("StarkSidebarReportTask error:", e);
        }
    },
    
    StarkSidebarCheckSupport: function() {
        try {
            var supported = typeof tt !== 'undefined' && 
                          (typeof tt.navigateToScene !== 'undefined' || 
                           typeof tt.registerRetainSideBar !== 'undefined');
            return supported ? 1 : 0;
        } catch (e) {
            console.error("StarkSidebarCheckSupport error:", e);
            return 0;
        }
    },
    
    StarkSidebarIsFromSidebar: function() {
        try {
            if (typeof tt !== 'undefined' && tt.getLaunchOptionsSync) {
                var options = tt.getLaunchOptionsSync();
                if (options) {
                    if (options.scene === 'sidebar') return 1;
                    if (options.query && options.query.scene === 'sidebar') return 1;
                    if (options.sceneInfo && options.sceneInfo.scene === 'sidebar') return 1;
                }
            }
            return 0;
        } catch (e) {
            console.error("StarkSidebarIsFromSidebar error:", e);
            return 0;
        }
    }
});