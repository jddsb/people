const unityNamespace = require("./unity-namespace.js");
const wasmSplitValues = require('./webgl-wasm-split');
const {
  launchEventType,
  scaleMode
} = require('./plugin-config.js');
require('./webgl.framework.js');
require('./plugin-config.js');

const managerConfig = {
     DATA_CDN: "$DEPLOY_URL",
     DATA_FILE_MD5: "$DATA_MD5", 
     CODE_FILE_MD5: "$CODE_MD5",
     GAME_NAME: "$GAME_NAME",
     APPID: "$APP_ID",
     DATA_FILE_SIZE: "$DATA_FILE_SIZE",
     OPT_DATA_FILE_SIZE: "$OPT_DATA_FILE_SIZE",
     useDataCDNAsStreamingAssetsUrl: $USE_DATA_CDN,
	
     loadDataPackageFromSubpackage: $LOAD_DATA_FROM_SUBPACKAGE,
     compressDataPackage: $COMPRESS_DATA_PACKAGE,
	   ...wasmSplitValues,

	 
     preloadDataList: [
        // 'DATA_CDN/StreamingAssets/WebGL/textures_005b9e6b32e22099edc38cba5b3d11de',
        // '/WebGL/bundles_e1af572c458eda6944e73db25cae88d5'
        $PRELOAD_LIST,
    ],
    
    cpJsFiles: [
      $CPJSFILES
    ],
 
     urlCacheList: [
		$URL_CACHE_LIST
     ],
     dontCacheFileNames: [
		$DONT_CACHE_FILE_NAMES
     ]
};
GameGlobal.managerConfig = managerConfig;

let launchOptions = null;
let isFromSidebar = false;

function getLaunchOptions() {
  if (launchOptions) return launchOptions;
  
  try {
    if (typeof tt !== 'undefined' && tt.getLaunchOptionsSync) {
      launchOptions = tt.getLaunchOptionsSync();
      console.log("Launch options:", launchOptions);
      
      if (launchOptions.scene === 'sidebar' || 
          (launchOptions.query && launchOptions.query.scene === 'sidebar') ||
          (launchOptions.sceneInfo && launchOptions.sceneInfo.scene === 'sidebar')) {
        isFromSidebar = true;
        console.log("Game launched from sidebar");
      }
    }
  } catch (e) {
    console.warn("Failed to get launch options:", e);
  }
  
  return launchOptions;
}

function handleSidebarEntry() {
  if (!isFromSidebar) return;
  
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
    console.warn("Failed to handle sidebar entry:", e);
  }
}

function navigateToSidebar() {
  try {
    if (typeof tt !== 'undefined' && tt.navigateToScene) {
      tt.navigateToScene({
        scene: 'sidebar',
        success: function(res) {
          console.log("Navigate to sidebar success:", res);
        },
        fail: function(err) {
          console.error("Navigate to sidebar failed:", err);
        }
      });
    } else if (typeof tt !== 'undefined' && tt.switchTab) {
      tt.switchTab({
        url: '/pages/index/index',
        success: function(res) {
          console.log("Switch tab success:", res);
        },
        fail: function(err) {
          console.error("Switch tab failed:", err);
        }
      });
    }
  } catch (e) {
    console.warn("Failed to navigate to sidebar:", e);
  }
}

function registerRetainSideBar(data) {
  try {
    if (typeof tt !== 'undefined' && tt.registerRetainSideBar) {
      tt.registerRetainSideBar({
        scene: 'sidebar',
        data: data || {},
        success: function(res) {
          console.log("Register retain sidebar success:", res);
        },
        fail: function(err) {
          console.error("Register retain sidebar failed:", err);
        }
      });
    }
  } catch (e) {
    console.warn("Failed to register retain sidebar:", e);
  }
}

function enterRetainSideBar() {
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
    console.warn("Failed to enter retain sidebar:", e);
  }
}

function reportTaskComplete(taskId, data) {
  try {
    if (typeof tt !== 'undefined' && tt.reportSideBarTask) {
      tt.reportSideBarTask({
        taskId: taskId,
        data: data || {},
        success: function(res) {
          console.log("Report task complete:", res);
        },
        fail: function(err) {
          console.error("Report task complete failed:", err);
        }
      });
    }
  } catch (e) {
    console.warn("Failed to report task:", e);
  }
}

GameGlobal.sidebarManager = {
  getLaunchOptions: getLaunchOptions,
  isFromSidebar: function() { return isFromSidebar; },
  handleSidebarEntry: handleSidebarEntry,
  navigateToSidebar: navigateToSidebar,
  registerRetainSideBar: registerRetainSideBar,
  enterRetainSideBar: enterRetainSideBar,
  reportTaskComplete: reportTaskComplete
};

function main() {
  const UnityManager = requirePlugin('UnityPlugin/index.js');
  console.log("UnityManager.version = ", UnityManager.version);
  
  getLaunchOptions();
  
  const info = tt.getSystemInfoSync();
  const canvas = tt.createCanvas();
  canvas.width = info.screenWidth;
  canvas.height = info.screenHeight;

  Object.assign(managerConfig, {
    hideAfterCallmain: $HIDE_AFTER_CALLMAIN,
    
    disableLoadingPage: $DISABLE_LOADING_PAGE,
    loadingPageConfig: {
      designWidth: 0,
      designHeight: 0,
      scaleMode: scaleMode.default,
      textConfig: {
        firstStartText: '首次加载请耐心等待',
        downloadingText: ['正在加载资源'],
        compilingText: '编译中',
        initText: '初始化中',
        completeText: '开始游戏',
        textDuration: 1500,
        style: {
          bottom: $TEXTCONFIG_BOTTOM,
          height: $TEXTCONFIG_HEIGHT,
          width: $TEXTCONFIG_WIDTH,
          color: '#ffffff',
          fontSize: 13,
        },
      },
      barConfig: {
        style: {
          width: $BARCONFIG_WIDTH,
          height: $BARCONFIG_HEIGHT,
          padding: 2,
          bottom: $BARCONFIG_BOTTOM,
          backgroundColor: '#ffffff',
        },
      },
      iconConfig: {
        visible: true,
        style: {
          width: $ICONCONFIG_WIDTH,
          height: $ICONCONFIG_HEIGHT,
          bottom: $ICONCONFIG_BOTTOM,
        },
      },
      materialConfig: {
        backgroundImage: 'images/background.png',
        iconImage: 'images/unity_logo.png',
      },
    },
  });

  const gameManager = new UnityManager(canvas, managerConfig, unityNamespace);
  
  gameManager.onLaunchProgress((e) => {
    if (e.type === launchEventType.launchPlugin) { }
    if (e.type === launchEventType.loadWasm) { }
    if (e.type === launchEventType.compileWasm) { }
    if (e.type === launchEventType.loadAssets) { }
    if (e.type === launchEventType.readAssets) { }
    if (e.type === launchEventType.prepareGame) { 
      handleSidebarEntry();
    }
  });

  gameManager.onModulePrepared(() => {
    console.log("Unity module prepared, sidebar status:", isFromSidebar);
  });

  gameManager.onLogError = function (err) {
    console.error(err);
  };

  globalThis.gameManager = gameManager;
  gameManager.startGame();
}

main();