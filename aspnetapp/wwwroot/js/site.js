// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
init();
function init() {
  $.ajax("/api/count", {
    method: "get",
  }).done(function (res) {
    if (res && res.data !== undefined) {
      $(".count-number").html(res.data);
    }
  });
}
function set(action) {
  $.ajax("/api/count", {
    method: "POST",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify({
      action: action,
    }),
  }).done(function (res) {
    if (res && res.data !== undefined) {
      $(".count-number").html(res.data);
    }
  });
}

function reverseText() {
  var text = $("#reverse-input").val();
  $.ajax("/api/reverse", {
    method: "POST",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify({ text: text }),
  }).done(function (res) {
    if (res && res.reversed !== undefined) {
      $("#reverse-result").html(res.reversed);
    } else {
      $("#reverse-result").html("出错了");
    }
  }).fail(function() {
    $("#reverse-result").html("出错了");
  });
}

// AI 聊天相关
var chatHistory = [];

function renderChat() {
  var $history = $("#chat-history");
  $history.empty();
  chatHistory.forEach(function(msg) {
    var align = msg.role === 'user' ? 'right' : 'left';
    var bg = msg.role === 'user' ? '#d1f5d3' : '#fff';
    var who = msg.role === 'user' ? '我' : 'AI';
    $history.append(
      '<div style="text-align:' + align + '; margin-bottom: 8px;">' +
        '<span style="display:inline-block; background:' + bg + '; border-radius:6px; padding:8px 12px; max-width:80%; word-break:break-all;">' +
        '<b>' + who + '：</b>' + msg.content + '</span>' +
      '</div>'
    );
  });
  $history.scrollTop($history[0].scrollHeight);
}

function sendMessage() {
  var input = $("#chat-input").val().trim();
  if (!input) return;
  $("#chat-input").val("");
  chatHistory.push({ role: 'user', content: input });
  renderChat();
  $("#send-btn").prop('disabled', true).text('发送中...');
  $.ajax("/api/chat", {
    method: "POST",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify({ messages: chatHistory }),
  }).done(function (res) {
    if (res && res.reply) {
      chatHistory.push({ role: 'assistant', content: res.reply });
      renderChat();
    }
  }).always(function() {
    $("#send-btn").prop('disabled', false).text('发送');
  });
}

$(document).on('click', '#send-btn', sendMessage);
$(document).on('keydown', '#chat-input', function(e) {
  if (e.key === 'Enter') {
    sendMessage();
    e.preventDefault();
  }
});
