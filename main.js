const { app, BrowserWindow, ipcMain, dialog, shell } = require('electron')
const path = require('node:path')
const { spawn, exec } = require('child_process');
const fs = require('fs');
const decoder = new TextDecoder('utf-8');

var child = null
var runing = false
var ss = {
  javaPath: 'java',
  jarPath: 'server.jar',
  memory: '2048',
  stopCommand: 'stop'
}

function createWindow() {
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false
    }
  })

  mainWindow.on('close', function (e) {
    if (runing) {
      e.preventDefault()
      dialog.showMessageBox(mainWindow, {
        type: 'warning',
        buttons: ['确定', '取消'],
        title: '提示',
        message: '服务器正在运行，是否强制停止？',
        cancelId: 1
      }).then((result) => {
        if (result.response === 0) {
          try { exec(`taskkill /pid ${child.pid} /T /F`) } catch (e) { }
          mainWindow.destroy()
        }
      })
    }
  })

  ipcMain.on('getState', (event) => {
    mainWindow.webContents.send('state', {
      runing: runing
    })
  })

  ipcMain.on('start', (event) => {
    if (!runing) {
      runing = true
      mainWindow.webContents.send('state', {
        runing: runing
      })
      const options = {
        cwd: path.join(ss.jarPath, '..')
      };
      child = spawn(ss.javaPath, [`-Xms${ss.memory}M`, `-Xmx${ss.memory}M`, '-jar', ss.jarPath, "--nogui"], options)
      child.stdout.on('data', (data) => {
        mainWindow.webContents.send('output', decoder.decode(data))
      })
      child.stderr.on('data', (data) => {
        mainWindow.webContents.send('output', decoder.decode(data))
      })
      child.on('exit', (code) => {
        runing = false
        mainWindow.webContents.send('state', {
          runing: runing
        })
      })
      child.on('error', (error) => {
        try { exec(`taskkill /pid ${child.pid} /T /F`) } catch (e) { }
        runing = false
        mainWindow.webContents.send('state', {
          runing: runing
        })
        dialog.showMessageBox(mainWindow, {
          type: 'error',
          buttons: ['确定'],
          title: '错误',
          message: error.message
        })
      })
    }
  })

  ipcMain.on('stop', (event, data) => {
    child.stdin.write(`${ss.stopCommand}\n`)
  })

  ipcMain.on('exec', (event, data) => {
    child.stdin.write(`${data}\n`)
    mainWindow.webContents.send('output', `> ${data}`)
  })

  ipcMain.handle('getSettings', async (event, data) => {
    return ss
  })

  ipcMain.on('saveSettings', (event, data) => {
    if (data.javaPath == '' || data.jarPath == '' || data.memory == '' || data.stopCommand == '') {
      dialog.showMessageBox(mainWindow, {
        type: 'error',
        buttons: ['确定'],
        title: '错误',
        message: '请填写所有选项'
      })
      return
    }
    ss.javaPath = data.javaPath
    ss.jarPath = data.jarPath
    ss.memory = data.memory
    ss.stopCommand = data.stopCommand
    mainWindow.loadFile('index.html')
  })

  ipcMain.handle('selectJava', async (event, data) => {
    const result = await dialog.showOpenDialog(mainWindow, {
      properties: ['openFile'],
      filters: [{
        name: 'java',
        extensions: ['exe']
      }]
    })
    return result.filePaths[0]
  })

  ipcMain.handle('selectJar', async (event, data) => {
    const result = await dialog.showOpenDialog(mainWindow, {
      properties: ['openFile'],
      filters: [{
        name: 'jar',
        extensions: ['jar']
      }]
    })
    return result.filePaths[0]
  })

  ipcMain.on('openServerFolder', async (event, data) => {
    const result = await dialog.showMessageBox(mainWindow, {
      type: 'question',
      buttons: ['主目录', '插件目录', 'mod目录'],
      title: '选择',
      message: '你要打开服务器的哪个文件夹？',
      cancelId: 3
    })
    switch (result.response) {
      case 0:
        shell.openPath(path.join(ss.jarPath, '..'))
        break
      case 1:
        shell.openPath(path.join(ss.jarPath, '..', 'plugins'))
        break
      case 2:
        shell.openPath(path.join(ss.jarPath, '..', 'mods'))
        break
      case 3:
        break
    }
  })

  mainWindow.loadFile('index.html')
}

app.whenReady().then(() => {
  try {
    const temp = JSON.parse(fs.readFileSync(path.join(app.getPath('userData'), 'settings.json'), 'utf-8'))
    ss.javaPath = temp.javaPath || 'java'
    ss.jarPath = temp.jarPath || 'server.jar'
    ss.memory = temp.memory || '2048'
    ss.stopCommand = temp.stopCommand || 'stop'
  } catch (e) { }

  createWindow()

  app.on('activate', function () {
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
})

app.on('window-all-closed', function () {
  fs.writeFileSync(path.join(app.getPath('userData'), 'settings.json'), JSON.stringify(ss))
  if (process.platform !== 'darwin') app.quit()
})
