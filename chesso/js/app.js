
var wsUri = "ws://localhost:3000/websocket";

var sendMessage = function(websocket,msg) {
  websocket.send(msg);
}

var initGame = function (playerColor,websocket) {

    var handleMove = function(source, target) {

      var move = game.move({from: source, to: target});

      if (move === null) { 
        return 'snapback';
      } else {
         sendMessage(websocket, $.toJSON({move: move, board: game.fen()}));
      }
    };

    var onDragStart = function(source, piece, position, orientation) {
      if (game.game_over() === true ||
          (game.turn() === 'w' && piece.search(/^b/) !== -1) ||
          (game.turn() === 'b' && piece.search(/^w/) !== -1) ||
          (game.turn() !== playerColor[0])) {
        return false;
      }
    };

    var cfg = {
        draggable: true,
        showNotation: false,
        position: 'start',
        orientation: playerColor,
        onDrop: handleMove,
        onDragStart: onDragStart
    };

    board = new ChessBoard('gameBoard', cfg);
    game = new Chess();
}

var onMessage = function(msg){
  // update board with adversari's move
  console.log("received: " + msg.data);
  let jso = $.parseJSON(msg.data);
  game.move(jso.move);
  board.position(game.fen());
}; 

var init = function() {
  var gameId = $("#gameId").val();
  var playerColor = $("#playerColor").val();;
  var websocket = new WebSocket(wsUri + '/' + gameId);
  websocket.onmessage = onMessage;
  initGame(playerColor, websocket);
}

$(document).ready(init);