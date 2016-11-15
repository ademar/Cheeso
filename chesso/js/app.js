
var playerColor = "w";

var initGame = function () {
    var cfg = {
        draggable: true,
        position: 'start',
        onDrop: handleMove,
        onDragStart: onDragStart
    };

    board = new ChessBoard('gameBoard', cfg);
    game = new Chess();
}

var handleMove = function(source, target) {

  var move = game.move({from: source, to: target});

  if (move === null) { 
    return 'snapback';
  } else {
     // send move to the server
     //socket.emit('move', {move: move, gameId: serverGame.id, board: game.fen()});
  }
}

var onDragStart = function(source, piece, position, orientation) {
  if (game.game_over() === true ||
      (game.turn() === 'w' && piece.search(/^b/) !== -1) ||
      (game.turn() === 'b' && piece.search(/^w/) !== -1) ||
      (game.turn() !== playerColor[0])) {
    return false;
  }
}; 

var init = function() {
  initGame("hello");
}

$(document).ready(init);