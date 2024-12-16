const { ipcRenderer } = require("electron")
window.$ = window.jQuery = require("./node_modules/jquery/dist/jquery.min.js");

$(document).ready(async function () {
  const ss = await ipcRenderer.invoke("getSettings")
  $("#javaPathBox").val(ss.javaPath)
  $("#jarPathBox").val(ss.jarPath)
  $("#memoryBox").val(ss.memory)
  $("#stopCommandBox").val(ss.stopCommand)
})

$("#selectJavaButton").click(async function () {
  const filePath = await ipcRenderer.invoke('selectJava')
  if (filePath) {
    $("#javaPathBox").val(filePath)
  }
})

$("#selectJarButton").click(async function () {
  const filePath = await ipcRenderer.invoke('selectJar')
  if (filePath) {
    $("#jarPathBox").val(filePath)
  }
})

$("#saveButton").click(function () {
  ipcRenderer.send("saveSettings", {
    javaPath: $("#javaPathBox").val(),
    jarPath: $("#jarPathBox").val(),
    memory: $("#memoryBox").val(),
    stopCommand: $("#stopCommandBox").val()
  })
})

$("#openServerFolderButton").click(function () {
  ipcRenderer.send("openServerFolder")
})
