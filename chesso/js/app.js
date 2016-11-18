
var wsUri = "ws://localhost:3000/websocket";

var playerColor = "w";

var sendMessage = function(websocket,msg) {
  websocket.send(msg);
}

var initGame = function (websocket) {

    var handleMove = function(source, target) {

		  var move = game.move({from: source, to: target});

		  if (move === null) { 
		    return 'snapback';
		  } else {
		     sendMessage(websocket, $.toJSON({move: move, gameId: "xxx-yyy-zzz", board: game.fen()}));
		  }
		};

    var cfg = {
        draggable: true,
        position: 'start',
        onDrop: handleMove,
        onDragStart: onDragStart
    };

    board = new ChessBoard('gameBoard', cfg);
    game = new Chess();
}

var onDragStart = function(source, piece, position, orientation) {
  if (game.game_over() === true ||
      (game.turn() === 'w' && piece.search(/^b/) !== -1) ||
      (game.turn() === 'b' && piece.search(/^w/) !== -1) ||
      (game.turn() !== playerColor[0])) {
    return false;
  }
};

var onMessage = function(msg){
  // update board with adversari's move
}; 

var init = function() {
  var websocket = new WebSocket(wsUri);
  websocket.onmessage = onMessage;
  initGame(websocket);
}

$(document).ready(init);