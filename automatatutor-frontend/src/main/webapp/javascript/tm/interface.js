/** Tahiti: hoverMenu basiert auf Nodes, schreibe HoverTM Labelabhängig. schaue wie zur hölle du Labels auf dem Fenster für die Maus erkennbar machst, siehe HoverMenuAlphabetCreation
 * @file Interface for drawing automata
 *
 * @author Matthew Weaver [mweaver223@gmail.com], Alexander Weinert [weinert@react.uni-saarland.de]
 * @param style Optional. The type of automaton-like thing to be drawn.
 *	May be one of 'detaut', 'nondetaut', 'buchigame', 'paritygame'.
 * 	Defaults to 'detaut' if none is given
 */
$.SvgCanvas = function(container, config, style) {

	if (style === undefined) style = 'detaut'

	var globalConfig = {
		node: {
			radius: 15,		// Used when rendering the node as a circle
			sideLength: 30	// Used when rendering the node as a square
		},
		hoverMenu: {
			step: Math.PI/6
		},
		transition: {
			loopWidth: Math.PI / 2,	// The distance between the two points at which a loop connects to its vertex (in radians)
			loopHeight: 80 // The height of a looping transition
		}
	}

	/**
	 * node.label: Function that gets the data of a node and returns the label of that node
	 * transition.labeled: Decides whether labels should be displayed on transitions.
	 	If true, transition.deterministic must be defined
	 * transition.deterministic: If true, there must be exactly one transition per label and no epsilon-transitions are allowed
	 * showInitialArrow: If true, the initial arrow is indicated with an arrow
	 */
	var styleConfig = {
		'detaut': {
			node: {
				label: 'id'
			},
			transition: {
				labeled: true,
				deterministic: true
			},
			hasInitialNode: true,
			twoPlayers: false,
			acceptanceMarker: 'css'
		},

        //Product automaton: States are labelled as tuples of states of the original automata
        'prodaut': {
            node: {
                label: 'tuple'
            },
            transition: {
                labeled: true,
                deterministic: true
            },
            hasInitialNode: true,
            twoPlayers: false,
            acceptanceMarker: 'css'
        },

        //Powerset automaton: State are labelled as sets of states of the original automaton
        'powaut': {
            node: {
                label: 'set',
                radius: 25
            },
            transition: {
                labeled: true,
                deterministic: true
            },
            hasInitialNode: true,
            twoPlayers: false,
            acceptanceMarker: 'css'
		},

		//Tahiti
		'tmaut': {
            node: {
                label: 'tm',
            },
            transition: {
                labeled: true,
                deterministic: true
            },
            hasInitialNode: true,
            twoPlayers: false,
            acceptanceMarker: 'css'
        },
		//Tohid Ende

		'nondetaut': {
			node: {
				label: 'id'
			},
			transition: {
				labeled: true,
				deterministic: false
			},
			hasInitialNode: true,
			twoPlayers: false,
			acceptanceMarker: 'css'
		},

		'buchigame': {
			node: {
				label: 'none'
			},
			transition: {
				labeled: false
			},
			hasInitialNode: false,
			twoPlayers: true,
			acceptanceMarker: 'border'
		},

		'buchicostgame': {
			node: {
				label: 'none'
			},
			transition: {
				labeled: true,
				deterministic: false
			},
			hasInitialNode: false,
			twoPlayers: true,
			acceptanceMarker: 'border'
		},

		'paritygame': {
			node: {
				label: 'priority'
			},
			transition: {
				labeled: false
			},
			hasInitialNode: false,
			twoPlayers: true,
			acceptanceMarker: 'none'
		},

		'paritycostgame': {
			node: {
				label: 'priority'
			},
			transition: {
				labeled: true,
				deterministic: false
			},
			hasInitialNode: false,
			twoPlayers: true,
			acceptanceMarker: 'none'
		}
	};

	$.extend(true, config, globalConfig, styleConfig[style])
	//Tahiti: config.node.label === 'tm' ||
	function useHoverMenu() { return config.transition.labeled === false || config.node.label === 'tm' || config.transition.deterministic === false || config.node.label === 'tuple' || config.node.label === 'set' }

	/// Returns true iff the hover menu around node d should be displayed
	function isHoverMenuVisible(d) { return (d.menu_visible && !newLink && !draggingLink && !draggingNode && showMenu)}
	//Tahiti hover visible
	function isHoverMenuTmOperationVisible(){/*console.log(showTmMenu && !newLink && !draggingLink && !draggingNode);*/ return (showTmMenu && !newLink && !draggingLink && !draggingNode)}
	//Tahiti openMenu
	function openAndPositionHoverTmOperationMenu(){
		//console.log("open Menu " + isHoverMenuTmOperationVisible())

		/*hoverMenuTmOperations.append('svg:circle')
		.attr('class', 'hoverMenu visible')
		.classed('visible', isHoverMenuTmOperationVisible())
		.attr('r', config.node.radius + 20)
		.on('mouseover', function(d) {
			showTmMenu = true;
			showMenu = true;
		})
		.on('mouseout', function(d) {
			//d.menu_visible = false;
			showTmMenu = false;
			showMenu = false;
			d3.select(this).classed('visible', false)
			restart();
		})
		.on('mousedown', function(d) {
			//d.menu_visible = false;
			restart();
		});*/
		//populateHoverMenuWithTmOperations(hoverMenuTmOperations)
		//console.log(hover_label_label.attr("x"))

		hoverMenuTmOperations.attr('transform', function() {
			return 'translate(' + hover_label_label.attr("x") + ',' + hover_label_label.attr("y") + ')';});
		restart();
	}
	//Tahiti fill tmOperationList
	function refreshTmOperationList(){
		//tmOperationList -> [[Start-Node, Ziel-Node, Lesezeichen-["","",""], Schreibzeichen-["","",""], Kopfbewegungen-["","",""]]
		//links.push({source: mousedown_node, target: mouseup_node, reflexive: refl, trans: drag_trans});
		tmOperationList = [];
		/*
		for(numberOfKanten)
			tmOperationList[i] = [];
		*/
	}
	/**
	 * Calculates the angle of an element at the given position when count many elements are distributed around a circle with step degrees inbetween them.
	 * Step must be given in radians. Return value is given in radians.
	 * Objects are centered around pi/4, i.e., the north of the circle.
	 */
	function calculateAngle(position, count, step) {
		return 3*Math.PI/2 - (count * config.hoverMenu.step / 2) + (position + .5) * config.hoverMenu.step;
	}

	function polarToPlanar(radius, angle) {
		return {
			x: radius * Math.cos(angle),
			y: radius * Math.sin(angle)
		}
	}

	function populateHoverMenusWithAlphabet(menus) {
		var symbolsToDistribute = (epsilonTrans ? alphabet.length - 1 : alphabet.length)

		function calculateXCoordinate(symbolIndex) {
			var angle = calculateAngle(symbolIndex, symbolsToDistribute, config.hoverMenu.step)
			if(epsilonTrans && symbolIndex === alphabet.length - 1) {
			    angle = Math.PI/2;
			}
			return polarToPlanar(config.node.radius + 10, angle).x;
		}

		function calculateYCoordinate(symbolIndex) {
			var angle = calculateAngle(symbolIndex, symbolsToDistribute, config.hoverMenu.step)
			if(epsilonTrans && symbolIndex === alphabet.length - 1) {
			    angle = Math.PI/2;
			}
			// Add 5 to the y-coordinate since it specifies the coordinate of the upper left corner
			return polarToPlanar(config.node.radius + 10, angle).y + 5;
	    }

	    function onLabelMouseover(node) {
	    	node.menu_visible = true;
			showMenu = true;
			restart();
	    }

	    function onLabelMousedown(node) {
	    	node.menu_visible = false;
			showMenu = false;
			newLink = true;
			mousedown_node = node;

			for(var j = 0; j < alphabet.length; j++){
			    drag_trans[j] = false;
			}

			drag_trans[alphabet.indexOf(this.textContent)] = true;

			drag_line
			    .style('marker-end', 'url(#end-arrow)')
			    .classed('hidden', false)
			    .attr('d', 'M' + mousedown_node.x + ',' + mousedown_node.y + 'L' + mousedown_node.x + ',' + mousedown_node.y);
			drag_label
			    .text(function(d) { return makeLabel(drag_trans); })
			    .classed('hidden', false);				

			restart();
	    }

	    function onLabelMouseout(nodeData) { showMenu = false }

		for(var i = 0; i < alphabet.length; i++){
			menus.append('svg:text')
			    .attr('class', 'hoverMenu visible')
			    .classed('visible', isHoverMenuVisible)
			    .text(alphabet[i])
			    .attr('x', calculateXCoordinate(i))
			    .attr('y', calculateYCoordinate(i))
			    .on('mouseover', onLabelMouseover)
			    .on('mousedown', onLabelMousedown)
			    .on('mouseout', onLabelMouseout);
	    }
	}

    //TODO:
	function populateHoverMenusWithNumbersBothSides(menus) {
        var symbolsToDistributeLeft = numberOfNodesOfAutomaton1
        var symbolsToDistributeRight = numberOfNodesOfAutomaton2

        	function calculateXCoordinateLeft(index) {
        		var angle = 3*Math.PI/2 - (Math.PI/2 - symbolsToDistributeLeft*config.hoverMenu.step/2) - (index + .5) * config.hoverMenu.step;
        		return polarToPlanar(config.node.radius + 10, angle).x;
        	}
        	function calculateYCoordinateLeft(index) {
               	var angle = 3*Math.PI/2 - (Math.PI/2 - symbolsToDistributeLeft*config.hoverMenu.step/2) - (index + .5) * config.hoverMenu.step;
                return polarToPlanar(config.node.radius + 10, angle).y + 5;
            }

        	function calculateXCoordinateRight(index) {
                var angle = 3*Math.PI/2 + (Math.PI/2 - symbolsToDistributeRight*config.hoverMenu.step/2) + (index + .5) * config.hoverMenu.step;
                return polarToPlanar(config.node.radius + 10, angle).x;
            }
            function calculateYCoordinateRight(index) {
                var angle = 3*Math.PI/2 + (Math.PI/2 - symbolsToDistributeRight*config.hoverMenu.step/2) + (index + .5) * config.hoverMenu.step;
                return polarToPlanar(config.node.radius + 10, angle).y + 5;
            }

        	function onLabelMouseover(node) {
        		node.menu_visible = true;
        		showMenu = true;
        		restart();
        	}

        	function onLabelMousedown(node) {
        	   	node.menu_visible = false;
        		showMenu = false;
        		newLink = true;
        		mousedown_node = node;

                if(this.getAttribute('pos') === 'left')
        		    mousedown_node.left = this.textContent;
        		else
        		    mousedown_node.right = this.textContent;

        		restart();
        	}

        	function onLabelMouseout(nodeData) { showMenu = false }

        	for(var i = 0; i < symbolsToDistributeLeft; i++){
        		menus.append('svg:text')
        		    .attr('class', 'hoverMenu visible')
        		    .classed('visible', isHoverMenuVisible)
        		    .text(i)
        		    .attr('pos', 'left')
        		    .attr('x', calculateXCoordinateLeft(i))
        		    .attr('y', calculateYCoordinateLeft(i))
        		    .on('mouseover', onLabelMouseover)
        		    .on('mousedown', onLabelMousedown)
        		    .on('mouseout', onLabelMouseout);
        	}

        	for(var i = 0; i < symbolsToDistributeRight; i++){
                menus.append('svg:text')
                    .attr('class', 'hoverMenu visible')
                    .classed('visible', isHoverMenuVisible)
                    .text(i)
                    .attr('pos', 'right')
                    .attr('x', calculateXCoordinateRight(i))
                    .attr('y', calculateYCoordinateRight(i))
                    .on('mouseover', onLabelMouseover)
                    .on('mousedown', onLabelMousedown)
                    .on('mouseout', onLabelMouseout);
            }
	}
	//Tahiti populateHoverMenu
	function populateHoverMenusWithAlphabetTMVersion(menus) {
		var symbolsToDistribute = (epsilonTrans ? alphabetSingle.length - 1 : alphabetSingle.length)

		function calculateXCoordinate(symbolIndex) {
			var angle = calculateAngle(symbolIndex, symbolsToDistribute, config.hoverMenu.step)
			if(epsilonTrans && symbolIndex === alphabetSingle.length - 1) {
			    angle = Math.PI/2;
			}
			return polarToPlanar(config.node.radius + 10, angle).x;
		}

		function calculateYCoordinate(symbolIndex) {
			var angle = calculateAngle(symbolIndex, symbolsToDistribute, config.hoverMenu.step)
			if(epsilonTrans && symbolIndex === alphabetSingle.length - 1) {
			    angle = Math.PI/2;
			}
			// Add 5 to the y-coordinate since it specifies the coordinate of the upper left corner
			return polarToPlanar(config.node.radius + 10, angle).y + 5;
	    }

	    function onLabelMouseover(node) {
	    	node.menu_visible = true;
			showMenu = true;
			//console.log(readingSymbolsCounter);
			//console.log(alphabet + " <-alphabet|alphabetSingle-> " + alphabetSingle);
			restart();
	    }
		//function onLabelClick(node){
		//	if(readingSymbolsCounter.length < numberOfTapes){
		//		readingSymbolsCounter = readingSymbolsCounter + this.textContent;
		//		if(readingSymbolsCounter.length < numberOfTapes)
		//			return;
		//	}
		//}
		function onLabelMousedown(node) {//choosingTMTransition
			if(choosingTMTransition)
				return;
			//numberOfTapes = 3;
			if(readingSymbolsCounter.length < (numberOfTapes)){
				readingSymbolsCounter = readingSymbolsCounter + this.textContent;
				choosingTMTransition = true;
				//console.log(readingSymbolsCounter);
				if(readingSymbolsCounter.length < numberOfTapes)
					return;
			}
			//window.alert("redingSymbolsCounter: " + readingSymbolsCounter + " tapenumber: " + numberOfTapes);
	    	node.menu_visible = false;
			showMenu = false;
			newLink = true;
			mousedown_node = node;

			for(var j = 0; j < alphabet.length; j++){
			    drag_trans[j] = false;
			}

			drag_trans[alphabet.indexOf(readingSymbolsCounter)] = true;

			drag_line
			    .style('marker-end', 'url(#end-arrow)')
			    .classed('hidden', false)
			    .attr('d', 'M' + mousedown_node.x + ',' + mousedown_node.y + 'L' + mousedown_node.x + ',' + mousedown_node.y);
			drag_label
			    .text(function(d) { return makeLabel(drag_trans); })
			    .classed('hidden', false);				
			//choosingTMTransition = true;
			readingSymbolsCounter = "";
			restart();
	    }
		function onLabelMouseup(nodeData) {choosingTMTransition = false;}
	    function onLabelMouseout(nodeData) { showMenu = false; choosingTMTransition = false;}

		for(var i = 0; i < alphabetSingle.length; i++){
			menus.append('svg:text')
			    .attr('class', 'hoverMenu visible')
			    .classed('visible', isHoverMenuVisible)
			    .text(alphabetSingle[i])
			    .attr('x', calculateXCoordinate(i))
			    .attr('y', calculateYCoordinate(i))
			    .on('mouseover', onLabelMouseover)
			    .on('mousedown', onLabelMousedown)
				.on('mouseout', onLabelMouseout)
				.on('mouseup', onLabelMouseup);
				//.on('click', onLabelClick);
	    }
	}

	//Tahiti populateHoverMenu
	function populateHoverMenuWithTmOperations(menus) {
		var symbolsToDistributeLeft = alphabetSingle.length;
        var symbolsToDistributeRight = 3;

        	function calculateXCoordinateLeft(index) {
        		var angle = 3*Math.PI/2 - (Math.PI/2 - symbolsToDistributeLeft*config.hoverMenu.step/2) - (index + .5) * config.hoverMenu.step;
        		return polarToPlanar(config.node.radius + 10, angle).x;
        	}
        	function calculateYCoordinateLeft(index) {
               	var angle = 3*Math.PI/2 - (Math.PI/2 - symbolsToDistributeLeft*config.hoverMenu.step/2) - (index + .5) * config.hoverMenu.step;
                return polarToPlanar(config.node.radius + 10, angle).y + 5;
            }

        	function calculateXCoordinateRight(index) {
                var angle = 3*Math.PI/2 + (Math.PI/2 - symbolsToDistributeRight*config.hoverMenu.step/2) + (index + .5) * config.hoverMenu.step;
                return polarToPlanar(config.node.radius + 10, angle).x;
            }
            function calculateYCoordinateRight(index) {
                var angle = 3*Math.PI/2 + (Math.PI/2 - symbolsToDistributeRight*config.hoverMenu.step/2) + (index + .5) * config.hoverMenu.step;
                return polarToPlanar(config.node.radius + 10, angle).y + 5;
            }

        	function onLabelMouseover(node) {
				//node.menu_visible = true;
				showTmMenu = true;
        		showMenu = true;
        		restart();
        	}

        	function onLabelMousedown(node) {
				return;
        	   	//node.menu_visible = false;
        		showMenu = true;
        		//newLink = true;
        		//mousedown_node = node;

                if(this.getAttribute('pos') === 'left')
        		    hover_label_label.attr("writeOp", this.textContent);
        		else if(this.getAttribute('pos') === 'right')
					hover_label_label.attr("headOp", this.textContent);

        		restart();
        	}

			//function onLabelMouseup(nodeData) {choosingTMTransition = false; console.log(readingSymbolsCounter);}
			function onLabelMouseout(nodeData) { showTmMenu = false; showMenu = false;}

        	for(var i = 0; i < symbolsToDistributeLeft; i++){
        		menus.append('svg:text')
        		    .attr('class', 'hoverMenu visible')
        		    .classed('visible', isHoverMenuTmOperationVisible())
        		    .text(alphabetSingle[i])
        		    .attr('pos', 'left')
        		    .attr('x', calculateXCoordinateLeft(i))
        		    .attr('y', calculateYCoordinateLeft(i))
        		    .on('mouseover', onLabelMouseover)
        		    .on('mousedown', onLabelMousedown)
        		    .on('mouseout', onLabelMouseout);
        	}

            menus.append('svg:text')
                .attr('class', 'hoverMenu visible')
                .classed('visible', isHoverMenuTmOperationVisible())
                .text('L')
                .attr('pos', 'right')
                .attr('x', calculateXCoordinateRight(0))
                .attr('y', calculateYCoordinateRight(0))
                .on('mouseover', onLabelMouseover)
                .on('mousedown', onLabelMousedown)
				.on('mouseout', onLabelMouseout);
			menus.append('svg:text')
                .attr('class', 'hoverMenu visible')
                .classed('visible', isHoverMenuTmOperationVisible())
                .text('R')
                .attr('pos', 'right')
                .attr('x', calculateXCoordinateRight(1))
                .attr('y', calculateYCoordinateRight(1))
                .on('mouseover', onLabelMouseover)
                .on('mousedown', onLabelMousedown)
				.on('mouseout', onLabelMouseout);
			menus.append('svg:text')
                .attr('class', 'hoverMenu visible')
                .classed('visible', isHoverMenuTmOperationVisible())
                .text('N')
                .attr('pos', 'right')
                .attr('x', calculateXCoordinateRight(2))
                .attr('y', calculateYCoordinateRight(2))
                .on('mouseover', onLabelMouseover)
                .on('mousedown', onLabelMousedown)
                .on('mouseout', onLabelMouseout);
            
	}

	//Tahiti populateHoverMenu
	function populateHoverMenusWithTmOperations(menus) {
		var symbolsToDistributeLeft = alphabetSingle.length;
        var symbolsToDistributeRight = 3;

		var symbolsToDistribute = 1
		//var activeTape = 1
		//console.log("wieder auf 1");

		function calculateXCoordinate(symbolIndex) {
			var angle = calculateAngle(symbolIndex, symbolsToDistribute, config.hoverMenu.step)
			if(epsilonTrans && symbolIndex === alphabetSingle.length - 1) {
				angle = Math.PI/2;
			}
			return polarToPlanar(config.node.radius + 10, angle).x;
		}
		
		function calculateYCoordinate(symbolIndex) {
			var angle = calculateAngle(symbolIndex, symbolsToDistribute, config.hoverMenu.step)
			if(epsilonTrans && symbolIndex === alphabetSingle.length - 1) {
				angle = Math.PI/2;
			}
			// Add 5 to the y-coordinate since it specifies the coordinate of the upper left corner
			return polarToPlanar(config.node.radius + 10, angle).y + 5;
		}

		function calculateXCoordinateLeft(index) {
			var angle = 3*Math.PI/2 - (Math.PI/2 - symbolsToDistributeLeft*config.hoverMenu.step/2) - (index + .5) * config.hoverMenu.step;
			return polarToPlanar(config.node.radius + 10, angle).x;
		}
		function calculateYCoordinateLeft(index) {
			var angle = 3*Math.PI/2 - (Math.PI/2 - symbolsToDistributeLeft*config.hoverMenu.step/2) - (index + .5) * config.hoverMenu.step;
			return polarToPlanar(config.node.radius + 10, angle).y + 5;
		}

		function calculateXCoordinateRight(index) {
			var angle = 3*Math.PI/2 + (Math.PI/2 - symbolsToDistributeRight*config.hoverMenu.step/2) + (index + .5) * config.hoverMenu.step;
			return polarToPlanar(config.node.radius + 10, angle).x;
		}
		function calculateYCoordinateRight(index) {
			var angle = 3*Math.PI/2 + (Math.PI/2 - symbolsToDistributeRight*config.hoverMenu.step/2) + (index + .5) * config.hoverMenu.step;
			return polarToPlanar(config.node.radius + 10, angle).y + 5;
		}

		function onLabelMouseover(node) {
			//node.menu_visible = true;
			showTmMenu = true;
			showMenu = true;
			endTmMenu = false;
			hoverOverTmOperation = true;
			restart();
		}

		function onLabelMousedown(node) {
			//node.menu_visible = false;
			//showMenu = false;
			//newLink = true;
			//mousedown_node = node;
			choosingTMOperations = true;
			//window.alert(this.text);
			if(numberOfTapes > 1){
				if(this.getAttribute('pos') === 'left'){
						//hover_label_label.attr("writeOp", this.textContent);
					var opList = hover_label_label.attr('writeOp');
					opList = opList.substr(0, activeTape-1) + this.textContent + opList.substr(activeTape);
					hover_label_label.attr("writeOp", opList);
				}
				else if(this.getAttribute('pos') === 'right'){
					var opList = hover_label_label.attr('headOp');
					opList = opList.substr(0, activeTape-1) + this.textContent + opList.substr(activeTape);
					hover_label_label.attr("headOp", opList);
				}
				else if(this.getAttribute('pos') === 'top'){
					activeTape = ( activeTape % numberOfTapes ) + 1;
					d3.select(this).text(activeTape);
					//console.log(activeTape);
				}
			}
			else{
				if(this.getAttribute('pos') === 'left')
					hover_label_label.attr("writeOp", this.textContent);
				else if(this.getAttribute('pos') === 'right')
					hover_label_label.attr("headOp", this.textContent);
			}
			changeTmOperationInList();

			//if(this.getAttribute('pos') === 'left')
			//    mousedown_node.left = this.textContent;
			//else
			//    mousedown_node.right = this.textContent;

			restart();
		}

		function onLabelMouseup(nodeData) { choosingTMOperations = false;/* showTmMenu = false; showMenu = false*/ }
		function onLabelMouseout(nodeData) { choosingTMOperations = false; showTmMenu = false; showMenu = false;/*console.log("WriteOp: " + hover_label_label.attr('writeOp') + " HeadOp: " + hover_label_label.attr('headOp'));*/ }
		
		if(numberOfTapes > 1){
			menus.append('svg:text')
				.attr('class', 'hoverMenu visible')
				.classed('visible', isHoverMenuTmOperationVisible)
				.text(activeTape)
				.attr('pos', 'top')
				.attr('x', calculateXCoordinate(0))
				.attr('y', calculateYCoordinate(0))
				.on('mouseover', onLabelMouseover)
				.on('mousedown', onLabelMousedown)
				.on('mouseup', onLabelMouseup)
				.on('mouseout', onLabelMouseout);
		}
		if(hover_label_label !== null)
		menus.append('svg:text')
			.attr('class', 'hoverMenu visible')
			.classed('visible', isHoverMenuTmOperationVisible)
			.text(hover_label_label.text())
			.attr('pos', 'mid')
			.attr('x', 0)
			.attr('y', 0)
			.on('mouseover', onLabelMouseover)
			.on('mousedown', onLabelMousedown)
			.on('mouseup', onLabelMouseup)
			.on('mouseout', onLabelMouseout);

		for(var i = 0; i < symbolsToDistributeLeft; i++){
			menus.append('svg:text')
				.attr('class', 'hoverMenu visible')
				.classed('visible', isHoverMenuTmOperationVisible)
				.text(alphabetSingle[i])
				.attr('pos', 'left')
				.attr('x', calculateXCoordinateLeft(i))
				.attr('y', calculateYCoordinateLeft(i))
				//.attr('transform', function() {if(hover_label_label === null) return; else if(alphabetSingle[i] === hover_label_label.attr('writeOp').charAt(activeTape-1)) return 'scale(1.1)'; else return 'scale(1.0)';})
				.style('fill', function() {if(hover_label_label === null) return 'black'; else if(alphabetSingle[i] === hover_label_label.attr('writeOp').charAt(activeTape-1)) return 'darkorange'; else return 'black';})
				.on('mouseover', onLabelMouseover)
				.on('mousedown', onLabelMousedown)
				.on('mouseup', onLabelMouseup)
				.on('mouseout', onLabelMouseout);
		}

		menus.append('svg:text')
			.attr('class', 'hoverMenu visible')
			.classed('visible', isHoverMenuTmOperationVisible)
			.text('L')
			.attr('pos', 'right')
			.attr('x', calculateXCoordinateRight(0))
			.attr('y', calculateYCoordinateRight(0))
			.style('fill', function() {if(hover_label_label === null) return 'black'; else if('L' === hover_label_label.attr('headOp').charAt(activeTape-1)) return 'darkorange'; else return 'black';})
			.on('mouseover', onLabelMouseover)
			.on('mousedown', onLabelMousedown)
			.on('mouseup', onLabelMouseup)
			.on('mouseout', onLabelMouseout);
		menus.append('svg:text')
			.attr('class', 'hoverMenu visible')
			.classed('visible', isHoverMenuTmOperationVisible)
			.text('R')
			.attr('pos', 'right')
			.attr('x', calculateXCoordinateRight(1))
			.attr('y', calculateYCoordinateRight(1))
			.style('fill', function() {if(hover_label_label === null) return 'black'; else if('R' === hover_label_label.attr('headOp').charAt(activeTape-1)) return 'darkorange'; else return 'black';})
			.on('mouseover', onLabelMouseover)
			.on('mousedown', onLabelMousedown)
			.on('mouseup', onLabelMouseup)
			.on('mouseout', onLabelMouseout);
		menus.append('svg:text')
			.attr('class', 'hoverMenu visible')
			.classed('visible', isHoverMenuTmOperationVisible)
			.text('N')
			.attr('pos', 'right')
			.attr('x', calculateXCoordinateRight(2))
			.attr('y', calculateYCoordinateRight(2))
			.style('fill', function() {if(hover_label_label === null) return 'black'; else if('N' === hover_label_label.attr('headOp').charAt(activeTape-1)) return 'darkorange'; else return 'black';})
			.on('mouseover', onLabelMouseover)
			.on('mousedown', onLabelMousedown)
			.on('mouseup', onLabelMouseup)
			.on('mouseout', onLabelMouseout);
		
	}

	function populateHoverMenusWithNumbers(menus) {

        var symbolsToDistribute = numberOfNodesOfAutomaton1

        function calculateXCoordinate(index) {
            var angle = 3*Math.PI/2 - (symbolsToDistribute-1)/2 * config.hoverMenu.step + index * config.hoverMenu.step;
            return polarToPlanar(config.node.radius + 10, angle).x;
        }
        function calculateYCoordinate(index) {
            var angle = 3*Math.PI/2 - (symbolsToDistribute-1)/2 * config.hoverMenu.step + index * config.hoverMenu.step;
            return polarToPlanar(config.node.radius + 10, angle).y + 5;
        }

        function onLabelMouseover(node){
            node.menu_visible = true
            showMenu = true
            restart();
        }
        function onLabelMousedown(node){
            node.menu_visible = false;
            showMenu = false;
        	newLink = true;
        	mousedown_node = node;

            if(mousedown_node.states.length === 0){
                for(var i = 0; i < symbolsToDistribute; i++){
                    mousedown_node.states.push(false);
                }
            }

        	mousedown_node.states[this.getAttribute('index')] = !mousedown_node.states[this.getAttribute('index')];
        }
        function onLabelMouseout(nodeData){
            showMenu = false
        }

        for(var i = 0; i < symbolsToDistribute; i++){
            menus.append('svg:text')
                .attr('class', 'hoverMenu visible')
                .classed('visible', isHoverMenuVisible)
                .text(i)
                .attr('index', i)
                .attr('x', calculateXCoordinate(i))
                .attr('y', calculateYCoordinate(i))
                .on('mouseover', onLabelMouseover)
                .on('mousedown', onLabelMousedown)
                .on('mouseout', onLabelMouseout);
        }
	}

	function populateHoverMenusWithUnlabeled(menus) {
		menus.append('svg:path')
		    .attr('class', 'link hoverMenu')
		    .classed('visible', isHoverMenuVisible)
		//    .attr('d','M0,0L15,0')
		//    .attr('transform','translate(' + (config.node.radius + 2) + ')')
		    .style('marker-end', 'url(#end-arrow)')
		    .on('mouseover', function(d) {
		    	d.menu_visible = true;
				showMenu = true;

				restart();
		    })
		    .on('mousedown', function(d) {
		    	d.menu_visible = false;
				showMenu = false;
				newLink = true;
				mousedown_node = d;

				for(var j = 0; j < alphabet.length; j++){
				    drag_trans[j] = false;
				}

				// Just pretend that we are dragging a transition labeled with the first symbol of the alphabet
				// in order to allow for uniform treatment of transition in the remainder of the code
				drag_trans[0] = true;

				drag_line
				    .style('marker-end', 'url(#end-arrow)')
				    .classed('hidden', false)
				    .attr('d', 'M' + mousedown_node.x + ',' + mousedown_node.y + 'L' + mousedown_node.x + ',' + mousedown_node.y);
				drag_label
				    .text(function(d) { return makeLabel(drag_trans); })
				    .classed('hidden', true);				

				restart();
		    })
		    .on('mouseout', function(d) {
				showMenu = false;
		    });
	}

    var Utils = this.Utils = function() {

		var _keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

		return {	   
			
		    "toXml": function(str) {
			return $('<p/>').text(str).html();
		    },
		    
		    "fromXml": function(str) {
			return $('<p/>').html(str).text();
		    },
		    
		    "convertToXMLReferences": function(input) {
			var output = '';
			for (var n = 0; n < input.length; n++){
			    var c = input.charCodeAt(n);
			    if (c < 128) {
				output += input[n];
			    }
			    else if(c > 127) {
				output += ("&#" + c + ";");
			    }
			}
			return output;
		    },
		    
		    "rectsIntersect": function(r1, r2) {
			return r2.x < (r1.x+r1.width) && 
			    (r2.x+r2.width) > r1.x &&
			    r2.y < (r1.y+r1.height) &&
			    (r2.y+r2.height) > r1.y;
		    },
		    
		    "snapToAngle": function(x1,y1,x2,y2) {
			var snap = Math.PI/4; // 45 degrees
			var dx = x2 - x1;
			var dy = y2 - y1;
			var angle = Math.atan2(dy,dx);
			var dist = Math.sqrt(dx * dx + dy * dy);
			var snapangle= Math.round(angle/snap)*snap;
			var x = x1 + dist*Math.cos(snapangle);  
			var y = y1 + dist*Math.sin(snapangle);
			return {x:x, y:y, a:snapangle};
		    },
		    
		    // TODO: This only works in firefox, find cross-browser compatible method
		    "text2xml": function(sXML) {
			var dXML = new DOMParser();
			dXML.async = false;
			var out = dXML.parseFromString(sXML, "text/xml");
			return out;
		    }
		}

    }();
    
    // set up SVG for D3
    var width = config.dimensions[0];
    var height = config.dimensions[1];



    var started = false;
    var locked = false;

    var svg = d3.select(container)
	.append('svg')
	.attr('width', width)
	.attr('height', height);

    // set up initial nodes and links
    var nodes = [],
	lastNodeId = -1,
	links = [],
	alphabet = [];

	var solveMode = false
	
	//Tahiti variable initiation
	//var readingSymbolsCounter = "";
	var choosingTMOperations = false;
	var choosingTMTransition = false;
	if(config.node.label === 'tm') {
		var tapeGraphics = svg.append('svg:g')
		var numberOfTapes = 1;
		var activeTape = 1;
		var tmModeIsFunction = true;
		var initialTmWriteOperation = [];
		var initialTmHeadOperation = [];
		var tapeList = [];
		var initTapeWords = [];
		var firstViewableTapeElement = [];
		var stepByStep = 0; 
		var tmOperationList = []; //[[Start-Node, Ziel-Node, Lesezeichen-["","",""], Schreibzeichen-["","",""], Kopfbewegungen-["","",""]]
		var hover_label_label = null;
		var showTmMenu = false;
		var transitionToEdit = null;
		var currentNode = null;
		var alphabetSingle = [];
		var readingSymbolsCounter = "";
		var tapeElementWidth = 30;
		var tapeElementHeight = 50;
		var tapeElementsPerLine = 20;
		var tapeHeadOffset = 4;
		var endTmMenu = true;
		var hoverOverTmOperation = false;
		var operationTextDisplay = svg.append('svg:text')
			.text("")
			.attr('x', 10)
			.attr('y', 30);
		var operationTextDisplayTapes = svg.append('svg:text')
			.text("")
			.attr('x', 10)
			.attr('y', 12);
		//var hoverMenuTmOperations = svg.append('svg:g');//.selectAll('g'),
		//tapeGraphics.selectAll("text").remove();
	}
    //for product construction
    var numberOfNodesOfAutomaton1 = 4,
    numberOfNodesOfAutomaton2 = 4;

    // init D3 force layout
    var force;

    // setup dragging behavior for nodes and links
    var draggingNode = false;             // true when a node is currently being dragged in interface
    var node_drag;                        // variable that ultimately contains dragging behavior for nodes in interface
    var draggingLink = false;             // true when dragging a transition
    var draggingEntire = false;           // true when dragging all transitions between two states
    var newLink = false;                  // true when a new link is being dragged in the interface
    var overTrash = false;                // true when the mouse is hovering over the trash icon
    var overClear = false;                // true when the mouse is hovering over the "clear all" button

    if(config.transition.labeled === false || config.transition.deterministic === false) {
    	var showMenu = false;                 // true when the NFA transition menu is being displayed
	}

	if(config.transition.labeled === true) {
		var epsilonTrans = !config.transition.deterministic;    // true when epsilon transitions are being used
	}

	if(config.transition.labeled === true) {
		var hover_label = false;              // true when hovering over label
	}

    //define trashbin
    var trashLabel = svg.append('svg:image')
	.attr('xlink:href', '../images/trash_bin.png')
	.attr('x', width - 60)
	.attr('y', height - 60)
	.attr('width', 50)
	.attr('height', 50);

    //define clear button
    var clearRect = svg.append('svg:rect')
	.attr('class', 'delete')
	.attr('width', 105)
	.attr('height', 26)
	.attr('x', width-104)
	.attr('y', -1)
	.on('mouseover', function() {
	    overClear = true;
	    clearRect.attr('width', 110)
		.attr('height', 29)
		.attr('x', width - 109);

	    clearText.attr('x', width - 100)
		.attr('y', 20);
	})
	.on('mouseout', function() {
	    overClear = false;

	    clearRect.attr('width', 105)
		.attr('height', 26)
		.attr('x', width - 104);

	    clearText.attr('x', width - 98)
		.attr('y', 18);
	})
	.on('mousedown', function() {
	    init();
	    restart();
	}); 
    var clearText = svg.append('svg:text')
	.text('Reset Canvas')
	.attr('x', width - 98)
	.attr('y', 18);

    // define arrow markers for graph links
    svg.append('svg:defs').append('svg:marker')
	.attr('id', 'end-arrow')
	.attr('viewBox', '0 -5 10 10')
	.attr('refX', 6)
	.attr('markerWidth', 4)
	.attr('markerHeight', 4)
	.attr('orient', 'auto')
	.append('svg:path')
	.attr('d', 'M0,-5L10,0L0,5')
	.attr('fill', '#000');

    // define initial state arrow
    var init_x1 = 0;
    var init_x2 = 0;
    var init_y = 0;
    var init_line = svg.append('svg:path')
		.attr('d', 'M' + init_x1 + ',' + init_y + ' L' + init_x2 + "," + init_y )
		.style('marker-end', 'url(#end-arrow)');

	if (config.hasInitialNode) {
		init_line.attr('class', 'link initLine')
	}
	else {
		init_line.attr('class', 'link initLine hidden')
	}

    // larger active areas for mouse events for varies parts of interface
    var hoverPath = svg.append('svg:g').selectAll('path'),
	path = svg.append('svg:g').selectAll('path'),
	hoverMenu = svg.append('svg:g').selectAll('g'),

	circle = svg.append('svg:g').selectAll('g'),
	labels = svg.append('svg:g').selectAll('text');//,
	//Tahiti hoverMenuTmOperations
	//hoverMenuTmOperations = svg.append('svg:g');//.selectAll('g'),
	if(config.node.label === 'tm'){
		var hoverMenuTmOperations = svg.append('svg:g');
	}

    var hiddenTrash = svg.append('svg:circle')
	.attr('class', 'hiddenTrash')
	.attr('cx', width)
	.attr('cy', height)
	.attr('r', 100)
	.on('mouseover', function() {
	    overTrash = true;

	    trashLabel.attr('x', width - 70)
		.attr('y', height - 80)
		.attr('width', 70)
		.attr('height', 70);

	    if(draggingLink && (!config.transition.deterministic || config.node.label === 'tm')) //Tahiti
		trash_link = mousedown_link;

	    return;
	})	
	.on('mouseout', function() {
	    overTrash = false;

	    trashLabel.attr('x', width - 60)
		.attr('y', height - 60)
		.attr('width', 50)
		.attr('height', 50);

	    trash_link = null;

	    return;
	})
	.on('mouseup', function() {
	    if(config.transition.deterministic && config.node.label !== 'tm') //Tahiti
		return;
	    if(draggingLink && draggingEntire){
			var toSplice = links.filter( function(l) {
				return (l.source === trash_link.source && l.target === trash_link.target);
			});

			toSplice.map(function(l) {
				links.splice(links.indexOf(l), 1);
			});

			//Tahiti
			if(config.node.label === 'tm')
				removeEntireTmOperationFromList(trash_link.source, trash_link.target);
	    } else if(draggingLink) {
			links.splice(links.indexOf(trash_link), 1);
			if(config.node.label === 'tm')
				removeTmOperationFromList(trash_link.source, trash_link.target, makeLabel(trash_link.trans));
		}
	    
	    draggingLink = false;
	    draggingEntire = false;
	    restart();
	});

    var showWarning = false;
    var warningText = svg.append('svg:text')
	.attr('class', 'warning hidden')
	.attr('text-anchor', 'middle')
	.attr('x', width/2)
	.attr('y', 30)
	.text("This text should be changed before displaying!");

    // vars related to line displayed when dragging new nodes
    var t = [];
    for(var i = 0; i < alphabet.length; i++) {
		t[i] = false;
	}
	//Tahiti
	if(config.node.label === 'tm'){
		t = [];
		for(var i = 0; i < alphabet.length; i++) {
			t[i] = false;
		}
	}
    var drag_trans = t;
    var drag_line = svg.append('svg:path')
		.attr('id', 'drag_line')
		.attr('class', 'link dragline hidden')
		.attr('d', 'M0,0L0,0');
    var drag_label = svg.append('svg:text')
		.text(function(d) { return makeLabel(drag_trans); })
		.attr('class', 'transLabel hidden')
		.attr('x', 0)
		.attr('y', 0);

    // mouse event vars
    var selected_node = null,    // Node selected by mouse in intervace
	hidden_link = null,      // Link being dragged (displayed invisible while dragging link is visible)
	mousedown_link = null,   // Link mousedown is over
	mousedown_node = null,   // Node mousdown is over
	mouseup_node = null,     // Node mouseup is over
	hover_node = null,       // Node mouse is hovering over
	hover_link = null,       // Link mouse is hovering over
	menu_node = null,        // Node selected by right click (for the displayed menu)
	menu_link = null,        // Link selected by right click (for the displayed menu)
	trash_link = null,       // Link currently dragged over trash
	mouse_x = null,          // x coordiante of mouse
	mouse_y = null,          // y coordinate of mouse
	old_target = null,       // previous target of transition being dragged
	initial_node;            // initial node of automata

    function resetMouseVars() {
	mousedown_node = null;
	mouseup_node = null;
	mousedown_link = null;
	old_target = null;
    }

    /**
     * Updates location of what is displayed by interface
     * (i.e. where the states and transitions are within the interface)
     * 
     */
    function tick() {

	//updates loc of init state arrow
	if(config.hasInitialNode) {
		init_x1 = initial_node.x - 48 - config.node.radius;
		init_x2 = initial_node.x - 5 - config.node.radius;
		init_y = initial_node.y;
		init_line.attr('d', 'M' + init_x1 + ',' + init_y + ' L' + init_x2 + "," + init_y );
	}

	// draws invisible wide paths for increased room when selecting links
	hoverPath.attr('d', drawPath);

	// draw directed edges with proper padding from node centers
	path.attr('d', drawPath);


	// draws labels above paths
	labels.attr('x', function(d) {//Tahiti x of labels if(config.node.label === 'tm' && numberOfTapes !== 1)
	    if(d.reflexive) {
			var angle = Math.PI / 2;
			if(d.source.flip) {
			    angle = 3 * Math.PI / 2;
			}
			var x = Math.round(d.source.x + config.node.radius * Math.cos(angle));
			var y = Math.round(d.source.y + config.node.radius * Math.sin(angle));
			var ax = Math.round(70 * Math.cos(angle + Math.PI / 4));
			var bx = Math.round(70 * Math.cos(angle - Math.PI / 4));
			if(config.node.label === 'tm' && numberOfTapes !== 1)
				return x + (ax + bx) / 2;
			if(d.source.reflexiveNum > 1) {
			    return x + (ax + bx) / 2 + ((d.linknum - (.5 + d.source.reflexiveNum/2 ))/d.source.reflexiveNum)*(15*d.source.reflexiveNum);
			}
			else {
			    return x + (ax + bx) / 2;
			}
	    }

	    var dx = d.target.x - d.source.x;
	    var dy = d.target.y - d.source.y;
	    var slope = dy / dx;
	    var angle = getAngle(dx, dy);
	    var nangle = angle + Math.PI / 2;
	    var edgeDeviation = 30;
	    var textDeviation = 20;
	    var edgeindex = d.linknum;
	    if(edgeindex > 0) {
			edgeindex = 1;
		}
	    if(d.flat) {
			edgeindex = 0;
		}
	    var deviation = edgeDeviation*edgeindex;
	    var textDev = ((deviation > 0) ? textDeviation : -textDeviation) + ((deviation * 3) / 4);
	    if(edgeindex === 0){
			if(d.source.x>d.target.x) {
			    textDev = textDev + 38;
			}
			else {
			    textDev = textDev + 18;
			}
	    }
	    edgeindex = d.linknum;
		var totindex = d.totnum;
		if(config.node.label === 'tm' && numberOfTapes !== 1)
			return (d.source.x + d.target.x) / 2 + Math.cos(nangle) * (textDev - 8);
	    if(totindex > 1) {
			return (d.source.x + d.target.x) / 2 + Math.cos(nangle) * (textDev - 8) + ((edgeindex - (.5 + totindex/2))/totindex)*(15*totindex);
		}
	    else {
			return (d.source.x + d.target.x) / 2 + Math.cos(nangle) * (textDev - 8);
		}
	})
    .attr('y', function(d) {//Tahiti y of labels if(config.node.label === 'tm' && numberOfTapes !== 1)
		if(d.reflexive) {
		    var angle = Math.PI / 2;
		    if(d.source.flip)
			angle = 3 * Math.PI / 2;
		    var x = Math.round(d.source.x + config.node.radius * Math.cos(angle));
		    var y = Math.round(d.source.y + config.node.radius * Math.sin(angle));
		    var ay = Math.round(70 * Math.sin(angle + Math.PI / 4));
		    var by = Math.round(70 * Math.sin(angle - Math.PI / 4));
		    if(y > d.source.y) {
				if(config.node.label === 'tm' && numberOfTapes !== 1)
					return (y + 10 + (ay + by) / 2) + 15 * (d.linknum - 1);
				return y + 10 + (ay + by) / 2;
			}
			else {
				if(config.node.label === 'tm' && numberOfTapes !== 1)
					return (y + (ay + by) / 2) - 15 * (d.linknum - 1);
		    	return y + (ay + by) / 2;
			}

		}

		var dx = d.target.x - d.source.x;
		var dy = d.target.y - d.source.y;
		var slope = dy / dx;
		var angle = getAngle(dx, dy);
		var nangle = angle + Math.PI / 2;
		var edgeDeviation = 30;
		var textDeviation = 20;
		var edgeindex = d.linknum;
		if(edgeindex > 0) {
		    edgeindex = 1;
		}
		if(d.flat) {
		    edgeindex = 0;
		}
		var deviation = edgeDeviation*edgeindex;
		var textDev = ((deviation > 0) ? textDeviation : -textDeviation) + ((deviation * 3) / 4);
		if(edgeindex === 0){
		    if(d.source.x > d.target.x)
			textDev = textDev + 25;
		    else
			textDev = textDev+15;
		}
		else if(d.source.x>d.target.x){
		    textDev = textDev-13;
		}
		else {
		    textDev = textDev -3;
		}

		if(config.node.label === 'tm' && numberOfTapes !== 1){
			var yOffset = 1;
			if(d.linknum > 1)
				yOffset = d.linknum;
			return ((d.source.y + d.target.y) / 2 + Math.sin(nangle) * textDev) - (15 * (yOffset - 1));
		}
		return (d.source.y + d.target.y) / 2 + Math.sin(nangle) * textDev;
	});

	//updates circle position
	circle.attr('transform', function(d) {
	    return 'translate(' + d.x + ',' + d.y + ')';
	});

	//updates hover menu position, if drawing an NFA
	if(useHoverMenu()){
	    hoverMenu.attr('transform', function(d) {
		return 'translate(' + d.x + ',' + d.y + ')';
	    });
	}
	//Tahiti hoverPosition & removal
	if(useHoverMenu() && config.node.label === 'tm' && hover_link != null && hover_label_label !== null){
		//var point = d3.mouse(this);
		transitionToEdit = hover_link;
	    hoverMenuTmOperations.attr('transform', function() {
		//return 'translate(' + mouse_x + ',' + mouse_y + ')';
		return 'translate(' + hover_label_label.attr("x") + ',' + hover_label_label.attr("y") + ')';
	    });
	}
	if(config.node.label === 'tm' && !showTmMenu && endTmMenu){
		//console.log("remove");
		hoverMenuTmOperations.selectAll('text').remove();
		hoverMenuTmOperations.selectAll('circle').remove();
	}
    }

    /**
     * Updates the contents of the interface
     *
     * In particular:
     *   adds/removes states and transitions in interface
     *   updates the various interface state variables (e.g. hover_node, etc...)
     */
    function restart() {
	linkNums(links);

	// updates/adds hoverPaths
	hoverPath = hoverPath.data(links);
	hoverPath.classed('hidden', function(d) { return d === hidden_link; });
	hoverPath.enter().append('svg:path')
	    .classed('hidden', function(d) { return d === hidden_link; })
	    .attr('class', 'link hoverPath')
	    .on('mouseover', function(d, i) {
		hover_link = d;
		menu_link = hover_link;

		var exp = path.filter(function(data) { return hover_link === data; });
		exp.classed('expanded', true);
		return;
	    })
	    .on('mouseout', function(d, i) {
		hover_link = d;

		var exp = path.filter(function(data) { return hover_link === data; });
		exp.classed('expanded', false);
		hover_link = null;
		return;
	    })
	    .on('mousedown', mousedownPath);
	hoverPath.exit().remove();

	// path (link) group
	path = path.data(links);
	// update existing links
	path.classed('hidden', function(d) { return d === hidden_link; })
	    .classed('expanded', function(d) {return d === hover_link; })
	    .style('marker-start', '')
	    .style('marker-end','url(#end-arrow)');
	// add new links
	path.enter().append('svg:path')
	    .attr('id', function(d, i) {return 'link' + i;})
	    .attr('class', 'link expanded')
	    .classed('hidden', function(d) { return d === hidden_link; })
	    .classed('expanded', function(d) {return d === hover_link; })
	    .style('marker-start', '')
	    .style('marker-end', 'url(#end-arrow)')
	    .on('mouseover', function(d) {
		hover_link = d;
		menu_link = hover_link;
		return;
	    })
	    .on('mouseout', function(d) {
		hover_link = null;
		return;
	    })
	    .on('mousedown', mousedownPath);
	// remove old links
	path.exit().remove();

	// add path labels
	labels = labels.data(links);

	labels.classed('hidden', function(d) { return d === hidden_link; })
	    .text(function(d) { return makeLabel(d.trans); });
	
	//Tahiti labels
	labels.enter().append('svg:text')
	    .attr('class', 'transLabel')
		.text(function(d) { return makeLabel(d.trans); })
		//.attr('writeOp',new Array(numberOfTapes))
		//.attr("headOp",new Array(numberOfTapes))
		.attr("writeOp", function(d) {if(config.node.label !== 'tm') return; var op = getTmOperations(d.source, d.target, makeLabel(d.trans)); /*console.log(op);*/ if(op !== null) return op[0]; return initialTmWriteOperation;})
		.attr("headOp", function(d) {if(config.node.label !== 'tm') return; var op = getTmOperations(d.source, d.target, makeLabel(d.trans)); if(op !== null) return op[1]; return initialTmHeadOperation})
	    .on('mouseover', function(d, i) {
		hover_link = d;
		menu_link = hover_link;
		hover_label = true; //Tahiti aktuell
		if(config.node.label === 'tm'){
			hover_label_label = d3.select(this);
			showTmMenu = true;
			openAndPositionHoverTmOperationMenu();
			operationTextDisplay.text(getVisualTmOperationFromLabel(hover_label_label));
		}
		//window.alert(d3.select(this).attr("transOp"));
		//window.alert(d3.select(this));
		return;
	    })
	    .on('mouseout', function(d, i) {
		hover_link = null;
		hover_label = false;
		//if(config.node.label === 'tm')
		//	hover_label_label = null;
		return;
	    })
	    .on('mousedown', function(d, i) {
		if(d3.event.button === 1 || d3.event.button === 2) return;
		if(draggingNode) return;

		// select link
		mousedown_link = d;
		hidden_link = mousedown_link;
		selected_node = null;

		draggingLink = true;
		mousedown_node = d.source;

		drag_trans = mousedown_link.trans;

		// displays drag_line
		drag_line
		    .style('marker-end', 'url(#end-arrow)')
		    .classed('hidden', false)
		    .attr('d', 'M' + mousedown_node.x + ',' + mousedown_node.y + 'L' + mousedown_node.x + ',' + mousedown_node.y);
		drag_label
		    .text(function(d) { return makeLabel(drag_trans); })
		    .classed('hidden', false);

		restart();
	    });

	labels.exit().remove();


	// circle (node) group
	circle = circle.data(nodes, function(d) { return d.id; });

	// update existing nodes (reflexive & selected visual states)
	if(config.acceptanceMarker === 'css') {
		circle.selectAll('circle#main')
		    .classed('accepting', function(d) { return d.accepting; });
		circle.selectAll('rect#main')
			.classed('accepting', function(d) { return d.accepting; });
	} else if (config.acceptanceMarker === 'border' ) {
		circle.selectAll('circle#marker')
			.classed('hidden', function(d) { return d.owner === 1 || !d.accepting; })
		circle.selectAll('rect#marker')
			.classed('hidden', function(d) { return d.owner === 0 || !d.accepting; })
	}

	switch(config.node.label) {
		case 'id':
			circle.selectAll('text').text(function (d) { return d.id })
			break
		//Tahiti
		case 'tm':
			circle.selectAll('text').text(function (d) { return d.id })
			circle.selectAll('circle').attr('transform', function(d) {if (currentNode !== null && d.id === currentNode.id) return 'scale(1.1)'; return 'scale(1.0)';})
			circle.selectAll('circle').style("stroke", function(d) {if (currentNode !== null && d.id === currentNode.id) return "red"; return "#2e5dea";})
			break
	    case 'tuple':
		    circle.selectAll('text').text(function (d) {return d.left + ',' + d.right })
		    break
		case 'set':
		    circle.selectAll('text').text(
		        function (d) {
		            var s = ''
		            for(var i = 0; i < d.states.length; i++){
		                if(d.states[i]){
		                    s += i + ',';
		                }
		            }
		            return s.slice(0, -1)
		        })
		    break
		case 'priority':
			circle.selectAll('text').text(function (d) { return d.priority })
			break
	}

	circle.selectAll('circle#main')
		.classed('hidden', function(d) { return d.owner === 1 })
		.classed('nonewinning', function(d) { return config.twoPlayers === true && d.winningPlayer === -1 })
		.classed('winningp0', function(d) { return config.twoPlayers === true && d.winningPlayer === 0 })
    	.classed('winningp1', function(d) { return config.twoPlayers === true && d.winningPlayer === 1 })
	circle.selectAll('rect#main')
		.classed('hidden', function(d) { return d.owner === 0 })
		.classed('nonewinning', function(d) { return config.twoPlayers === true && d.winningPlayer === -1 })
		.classed('winningp0', function(d) { return config.twoPlayers === true && d.winningPlayer === 0 })
    	.classed('winningp1', function(d) { return config.twoPlayers === true && d.winningPlayer === 1 })


	// add new nodes
	var g = circle.enter().append('svg:g');

	function onNodeMouseover(d) {
		// enlarge target node Tahiti steptodo
		hover_node = d;
		menu_node = hover_node;
		d.menu_visible = true;
		showMenu = true;
		d3.select(this).attr('transform', 'scale(1.1)');
		d3.select(this.parentNode).selectAll('#marker').attr('transform', 'scale(1.1)')
		restart();
		return;
	}

	function onNodeMouseout(d) {
		// unenlarge target node
		hover_node = null;
		showMenu = false;
		d3.select(this).attr('transform', '');
		d3.select(this.parentNode).selectAll('#marker').attr('transform', '')
		return;
	}

	function onNodeDblclick(d) {
		if(config.twoPlayers) {
			// Iterate through -1, 0, 1
			d.winningPlayer = ((d.winningPlayer + 2) % 3 )- 1
		} else {
			d.accepting = !d.accepting;
		}
		restart();
		return;
	}

	function onNodeMousedown(d) {
		if(d3.event.button === 1 || d3.event.button === 2) return;
		hidden_link = null;
		selected_node = d;

		if(!solveMode) {
			circle.call(node_drag);
			svg.classed('ctrl', true);
			draggingNode = true;
		}

		resetMouseVars();
		restart();
		return;
	}

	function onNodeMouseup(d) {
		circle
		    .on('mousedown.drag', null)
		    .on('touchstart.drag', null);
		svg.classed('ctrl', false);
		draggingNode = false;

		if(!mousedown_node) return;

		// needed by FF
		drag_line
		    .classed('hidden', true)
		    .style('marker-end', '');
		drag_label
		    .classed('hidden', true);

		// check for drag-to-self
		mouseup_node = d;

		// unenlarge target node
		d3.select(this).attr('transform', '');
			//Tahiti
		if(draggingEntire) {
		    // add link to graph (update if exists)
		    for(var i = 0; i < alphabet.length; i++) {
				if(drag_trans[i]){
				    var t = []
				    for(var j = 0; j < alphabet.length; j++){
						t[j] = false;
				    }
				    t[i] = true;
				    var refl = false;
				    if(mousedown_node === mouseup_node) {
					refl = true;
					mousedown_node.reflexiveNum++;
				    }
					links.push({source: mousedown_node, target: mouseup_node, reflexive: refl, trans: t});
					if(config.node.label === 'tm')
						changeEntireTmOperationTarget(mousedown_node, mousedown_link.target, mouseup_node);
				}
		    }

		    if(mousedown_link.reflexive)
			mousedown_link.source.reflexiveNum = mousedown_link.source.reflexiveNum - mousedown_link.trans.length;
		    links.splice(links.indexOf(mousedown_link), 1);
		}//Tahiti
		else if (newLink === true && (!config.transition.deterministic || config.node.label === 'tm')) {
		    
		    var multiplicityIssue = false;

		    for(var i = 0; i < links.length; i++) {
				var transIssue = false;
				for(var j = 0; j < alphabet.length; j++) {
				    if(links[i].trans[j] && drag_trans[j])
					transIssue = true;
				}//Tahiti
				if((links[i].source === mousedown_node && links[i].target === mouseup_node && transIssue) || (config.node.label === 'tm' && links[i].source === mousedown_node && transIssue)) {
				    multiplicityIssue = true;
				}
		    }	

		    var epsilonIssue = epsilonTrans && (mousedown_node === mouseup_node) && drag_trans[alphabet.length -1];

		    if(!multiplicityIssue && !epsilonIssue){
				var refl = false;
				if(mousedown_node === mouseup_node) {
				    refl = true;
				    mousedown_node.reflexiveNum++;
				}//Tahiti tbd links einfügen
				links.push({source: mousedown_node, target: mouseup_node, reflexive: refl, trans: drag_trans});
				if(config.node.label === 'tm')
					addInitialTmOperationToList(mousedown_node, mouseup_node, makeLabel(drag_trans));
			}
		}
		else {

			if(config.node.label === 'tm')
				changeTmOperationTarget(mousedown_node, mousedown_link.target, makeLabel(drag_trans), mouseup_node);

		    if(mousedown_link.reflexive) {
				mousedown_link.source.reflexiveNum--;
				mousedown_link.reflexive = false;
		    }

			mousedown_link.target = d;

		    if(mousedown_link.target === mousedown_link.source) {
				mousedown_link.source.reflexiveNum++;
				mousedown_link.reflexive = true;
		    }
		}
		var t = [];
		for(var i = 0; i < alphabet.length; i++) {
		    t[i] = false;
		}
		drag_trans = t;

		linkNums(links);

		draggingLink = false;
		draggingEntire = false;
		newLink = false;
		hidden_link = null;
		resetMouseVars();
		restart();
	}

    //rim of regular node
	g.append('svg:circle')
	    .attr('class', 'node')
	    .attr('id', 'main')
	    .attr('r', config.node.radius)
	    .style('stroke', '#5B90B2')
	    .classed('hidden', function(d) { return d.owner === 1 })
	    .classed('accepting', function(d) { return config.acceptanceMarker === 'css' && d.accepting; })
	    .classed('nonewinning', function(d) { return config.twoPlayers === true && d.winningPlayer === -1 })
	    .classed('winningp0', function(d) { return config.twoPlayers === true && d.winningPlayer === 0 })
	    .classed('winningp1', function(d) { return config.twoPlayers === true && d.winningPlayer === 1 })
	    .on('mouseover', onNodeMouseover)
	    .on('mouseout', onNodeMouseout)
	    .on('dblclick', onNodeDblclick)
	    .on('mousedown', onNodeMousedown)
	    .on('mouseup', onNodeMouseup);

	g.append('svg:circle')
		.attr('r', config.node.radius * 0.8)
		.attr('id', 'marker')
	    .style('stroke', '#5B90B2')
	    .style('fill-opacity', '0.0')
	    .style('stroke-width', '1px')
	    .style('pointer-events', 'none')
	    .classed('hidden', function(d) { return !(config.acceptanceMarker === 'border') || d.owner === 1 || d.accepting === false })

    g.append('svg:rect')
	    .attr('class', 'node')
	    .attr('id', 'main')
	    .attr('x', -config.node.sideLength / 2)
	    .attr('y', -config.node.sideLength / 2)
	    .attr('width', config.node.sideLength)
	    .attr('height', config.node.sideLength)
	    .style('stroke', '#5B90B2')
	    .classed('hidden', function(d) { return d.owner === 0 })
	    .classed('accepting', function(d) { return config.acceptanceMarker === 'css' && d.accepting; })
	    .classed('nonewinning', function(d) { return config.twoPlayers === true && d.winningPlayer === -1 })
	    .classed('winningp0', function(d) { return config.twoPlayers === true && d.winningPlayer === 0 })
	    .classed('winningp1', function(d) { return config.twoPlayers === true && d.winningPlayer === 1 })
	    .on('mouseover', onNodeMouseover)
	    .on('mouseout', onNodeMouseout)
	    .on('dblclick', onNodeDblclick)
	    .on('mousedown', onNodeMousedown)
	    .on('mouseup', onNodeMouseup);

	g.append('svg:rect')
		.attr('x', - config.node.sideLength / 2 + config.node.sideLength * (1 - 0.8) / 2)
		.attr('y', - config.node.sideLength / 2 + config.node.sideLength * (1 - 0.8) / 2)
		.attr('width', config.node.sideLength * 0.8)
	    .attr('height', config.node.sideLength * 0.8)
	    .attr('id', 'marker')
	    .style('stroke', '#5B90B2')
	    .style('fill-opacity', '0.0')
	    .style('stroke-width', '1px')
	    .style('pointer-events', 'none')
	    .classed('hidden', function(d) { return !(config.acceptanceMarker === 'border') || d.owner === 0 || d.accepting === false })

	// show node IDs
	var newLabels = g.append('svg:text')
		.attr('x', 0)
		.attr('y', 5)
		.attr('class', 'id')

	switch(config.node.label) {
		case 'id':
			newLabels.text(function (d) { return d.id });
			break
		//Tohid
		case 'tm':
			newLabels.text(function (d) { return d.id });
			break
		case 'tuple':
		    newLabels.text(function (d) { return d.left + ',' + d.right });
			break
	    case 'set':
            circle.selectAll('text').text(
        		function (d) {
        		    var s = ''
        		    for(var i = 0; i < d.states.length; i++){
        		        if(d.states[i]){
        		            s += i + ',';
        		        }
        		    }
        		    return s.slice(0, -1)
        		})
        	break
		case 'priority':
			newLabels.text(function (d) { return d.priority });
			break
	}

	// remove old nodes
	circle.exit().remove();

	// handles the hoverMenu, if drawing an NFA
	if(useHoverMenu() && !solveMode) {
	    hoverMenu = hoverMenu.data(nodes, function(d) { return d.id; });
	    hoverMenu.selectAll('circle')
	    	.classed('visible', isHoverMenuVisible);

	    // add new nodes
	    var menus = hoverMenu.enter()
	    	.append('svg:g');
	  
	    menus.append('svg:circle')
			.attr('class', 'hoverMenu visible')
			.classed('visible', isHoverMenuVisible)
			.attr('r', config.node.radius + 20)
			.on('mouseover', function(d) {
			    showMenu = true;
			})
			.on('mouseout', function(d) {
			    d.menu_visible = false;
				showMenu = false;
				//if(config.node.label === 'tm')
				//	readingSymbolsCounter = "";
			    restart();
			})
			.on('mousedown', function(d) {
			    d.menu_visible = false;
			    restart();
			});

		// Hide the transition labels if the hover menu is not active
	    hoverMenu.selectAll('text').classed('visible', isHoverMenuVisible);
	    // Hide the prototype transition if the hover menu is not active
		hoverMenu.selectAll('path').classed('visible', isHoverMenuVisible);
		
		//Tahiti
	    if(config.node.label === 'tuple'){
	        populateHoverMenusWithNumbersBothSides(menus)
	    } else if(config.node.label === 'set'){
			populateHoverMenusWithNumbers(menus)
		} else if(config.node.label === 'tm'){
	        populateHoverMenusWithAlphabetTMVersion(menus)
	    } else if(config.transition.labeled) {
	    	populateHoverMenusWithAlphabet(menus)
		} else {
			populateHoverMenusWithUnlabeled(menus)
		}

	    hoverMenu.exit().remove();
	}

	//Tahiti hoverMenu
	// handles the hoverMenu for operations, if drawing a TM
	if(config.node.label === 'tm' && (showTmMenu || hover_label ) && (!newLink && !draggingLink && !draggingNode)) {
		/*
		//console.log(isHoverMenuTmOperationVisible());
		hoveredLabelList = new Array();
		hoveredLabelList[0] = hover_label_label;
		*/
	    hoverMenuTmOperations = hoverMenuTmOperations.data(labels);//hoveredLabelList);//nodes, function(d) { return d.id; });
	    hoverMenuTmOperations.selectAll('circle')
	    	.classed('visible', isHoverMenuTmOperationVisible);
/*
	    // add new nodes
	    var menus = hoverMenuTmOperations.enter()
	    	.append('svg:g');
		//if(hover_label_label !== null){
		*/
		//hoverMenuTmOperations.selectAll('text').remove();
		//hoverMenuTmOperations.selectAll('circle').remove();
		endTmMenu = false;
	    hoverMenuTmOperations.append('svg:circle')
			.attr('class', 'hoverMenu visible')
			.classed('visible', isHoverMenuTmOperationVisible())
			.attr('r', config.node.radius + 20)
			.on('mouseover', function(d) {
				showTmMenu = true;
			    showMenu = true;
			})
			.on('mouseout', function(d) {
				//d.menu_visible = false;
				endTmMenu = true;
				showTmMenu = false;
			    showMenu = false;
				//restart();
				console.log("remove");
				//hoverMenuTmOperations.selectAll().remove();
				//d3.select(this).classed('visible', true)
				restart();
			})
			.on('mousedown', function(d) {
			    //d.menu_visible = false;
				//restart();
				//hoverMenuTmOperations.selectAll().remove();
				//d3.select(this).classed('visible', true)
				//endTmMenu = true;
				restart();
			});
		//}
		// Hide the transition labels if the hover menu is not active
	    hoverMenuTmOperations.selectAll('text').classed('visible', isHoverMenuTmOperationVisible());
	    // Hide the prototype transition if the hover menu is not active
		hoverMenuTmOperations.selectAll('path').classed('visible', isHoverMenuTmOperationVisible());
		
		populateHoverMenusWithTmOperations(hoverMenuTmOperations)

	    //hoverMenuTmOperations.exit().remove();
	}

	warningText.classed('hidden', function(){
	    return !showWarning;
	});

	// set the graph in motion
	force.start();
    }

    /**
     * Constructs strings for the d-argument of svg:path
     */
    function dBuilder() {

    	// Private variable that is used to chain the commands
    	var commands = []

    	/**
    	 * Takes one or two arguments. If there is only one argument, it is expected
    	 * to be an object with the keys "x" and "y" defined. If there are two arguments,
    	 * they are expected to be the x- and y-coordinates
    	 */
    	this.moveTo = function(x, y) {
    		if ( arguments.length === 1 ) { y = arguments[0].y; x = arguments[0].x }
    		commands.push("m" + x + "," + y)
    	},

    	/**
    	 * Takes one or two arguments. If there is only one argument, it is expected
    	 * to be an object with the keys "x" and "y" defined. If there are two arguments,
    	 * they are expected to be the x- and y-coordinates
    	 */
    	this.MoveTo = function(x, y) {
    		if ( arguments.length === 1 ) { y = arguments[0].y; x = arguments[0].x }
    		commands.push("M" + x + "," + y)
    	},

    	/**
    	 * Takes one or two arguments. If there is only one argument, it is expected
    	 * to be an object with the keys "x" and "y" defined. If there are two arguments,
    	 * they are expected to be the x- and y-coordinates
    	 */
    	this.lineTo = function(x, y) {
    		if ( arguments.length === 1 ) { y = arguments[0].y; x = arguments[0].x }
    		commands.push("l" + x + "," + y)
    	},

    	/**
    	 * Takes one or two arguments. If there is only one argument, it is expected
    	 * to be an object with the keys "x" and "y" defined. If there are two arguments,
    	 * they are expected to be the x- and y-coordinates
    	 */
    	this.LineTo = function(x, y) {
    		if ( arguments.length === 1 ) { y = arguments[0].y; x = arguments[0].x }
    		commands.push("L" + x + "," + y)
    	},

    	/**
    	 * Takes two or four arguments, which define the control- and endpoint of
    	 * the Bezier-curve. If there are two arguments, they are all expected to be objects
    	 * defining the keys "x" and "y". If there are four arguments, they are expected to be
    	 * the x- and y-coordinates of the control- and endpoint, in this order, i.e.,
    	 * startX, startY, controlX, controlY, endX, endY
    	 */
    	this.quadraticCurveTo = function(controlX, controlY, endX, endY) {
    		if ( arguments.length === 2 ) {
    			// In this case, the first two arguments store objects containing the coordinates
    			endY = arguments[1].y
    			endX = arguments[1].x
    			controlY = arguments[0].y
    			controlX = arguments[0].x
    		}

    		var command = ['q']
    		command.push([controlX, controlY].join(','))
    		command.push([endX, endY].join(','))

    		commands.push(command.join(' '))
    	},

    	/**
    	 * Takes two, three or six arguments, which define the first and second controlpoint as well as
    	 * the endpoint of the Bezier-curve. If there are two arguments, the first one shall be an object
    	 * defining the keys "1" and "2", both of which in turn are objects defining the keys "x" and "y".
    	 * If there are three arguments, they are all expected to be objects defining the keys "x" and "y".
    	 * If there are six arguments, they are expected to be the x- and y-coordinates of the first and
    	 * second controlpoint and the endpoint, in this order, i.e., control1X, control1Y, control2X,
    	 * control2Y, endX, endY
    	 */
    	this.cubicCurveTo = function(control1X, control1Y, control2X, control2Y, endX, endY) {
    		if ( arguments.length === 2 ) {
    			// In this case, the first two arguments store objects containing the coordinates
    			endY = arguments[1].y
    			endX = arguments[1].x
    			control2Y = arguments[0][2].y
    			control2X = arguments[0][2].x
    			control1Y = arguments[0][1].y
    			control1X = arguments[0][1].x
    		} else if ( arguments.length === 3 ) {
    			// In this case, the first three arguments store objects containing the coordinates
    			endY = arguments[2].y
    			endX = arguments[2].x
    			control2Y = arguments[1].y
    			control2X = arguments[1].x
    			control1Y = arguments[0].y
    			control1X = arguments[0].x
    		}

    		var command = ['c']
    		command.push([control1X, control1Y].join(','))
    		command.push([control2X, control2Y].join(','))
    		command.push([endX, endY].join(','))

    		commands.push(command.join(' '))
    	},

    	this.closePath = function() {
    		commands.push('z')
    	}

    	this.build = function() {
    		return commands.join(' ')
    	}
    }

    function drawSelfLoop(transition) {
    	transition.relfexive = true;

		var angle = Math.PI / 2;
		if(transition.source.flip) {
			angle = 3 * Math.PI / 2;
		}

		var loopHeight = config.transition.loopHeight
		var outAngle = angle + config.transition.loopWidth / 2
		var inAngle = angle - config.transition.loopWidth / 2

		var builder = new dBuilder()

		var startPoint = polarToPlanar(config.node.radius, angle)
		startPoint.x += transition.source.x
		startPoint.y += transition.source.y

		builder.moveTo(startPoint)

		var controlPoints = {
			1: polarToPlanar(loopHeight, outAngle),
			2: polarToPlanar(loopHeight, inAngle)
		}
		var endPoint = polarToPlanar(6, inAngle)

	    builder.cubicCurveTo(controlPoints, endPoint)

	    return builder.build()
    }

    function drawLink(transition) {
		var x1 = transition.source.x,
		    y1 = transition.source.y,
		    x2 = transition.target.x,
		    y2 = transition.target.y;

		var dx = x2 - x1;
		var dy = y2 - y1;
		var slope = dy / dx;
		var angle = getAngle(dx, dy);
		var nangle = angle + Math.PI / 2;

		var third1x = (2 * x1 + x2) / 3;
		var third1y = (2 * y1 + y2) / 3;
		var third2x = (x1 + 2 * x2) / 3;
		var third2y = (y1 + 2 * y2) / 3;

		var offSet = transition.totnum;
		var edgeDeviation = 30;
		var edgeindex = transition.linknum;
		if(edgeindex > 0)
		    edgeindex = 1;
		if(transition.flat)
		    edgeindex = 0;
		var deviation = edgeDeviation*edgeindex;

		var ay = third1y + Math.sin(nangle) * deviation;
		var ax = third1x + Math.cos(nangle) * deviation;
		var by = third2y + Math.sin(nangle) * deviation;
		var bx = third2x + Math.cos(nangle) * deviation;

		/**
		 * If low <= val <= high, returns val.
		 * Otherwise returns the interval boundary that val is closer to 
		 */
		function moveToBoundaries(val, low, high) {
			if (val < low) { return low }
			else if (high < val) { return high }
			else { return val }
		}

		var len1 = Math.sqrt(Math.pow(ax - x1, 2) + Math.pow(ay - y1, 2));
		// For circles, we can simply calculate the border points using polar coordinates
		// For squares, we circumscribe a cirle, calculate its border points, and then trim the coordinates to the edges of the square
		var radius1 = (transition.source.owner === 0) ? config.node.radius : (Math.sqrt(2*Math.pow(config.node.sideLength / 2, 2)))
		
		var boundary1x = x1 + radius1 * (ax - x1) / len1;
		var boundary1y = y1 + radius1 * (ay - y1) / len1;
		if (transition.source.owner === 1) {
			boundary1x = moveToBoundaries(boundary1x, x1 - config.node.sideLength / 2, x1 + config.node.sideLength / 2)
			boundary1y = moveToBoundaries(boundary1y, y1 - config.node.sideLength / 2, y1 + config.node.sideLength / 2)
		}

		var len2 = Math.sqrt(Math.pow(bx - x2, 2) + Math.pow(by - y2, 2));
		var radius2 = (transition.target.owner === 0) ? config.node.radius : (Math.sqrt(2*Math.pow(config.node.sideLength / 2, 2)))
		var boundary2x = x2 + (radius2 + 4) * (bx - x2) / len2;
		var boundary2y = y2 + (radius2 + 4) * (by - y2) / len2;
		if (transition.target.owner === 1) {
			boundary2x = moveToBoundaries(boundary2x, x2 - config.node.sideLength / 2 - 4, x2 + config.node.sideLength / 2 + 4)
			boundary2y = moveToBoundaries(boundary2y, y2 - config.node.sideLength / 2 - 4, y2 + config.node.sideLength / 2 + 4)
		}

		return 'M' + boundary1x + ',' + boundary1y + ' C' + ax + ',' + ay + ' ' + bx + ',' + by + ' ' + boundary2x + ',' + boundary2y;
    }

    /**
     * Calculates the formula for drawing paths
     *
     */
    function drawPath(d) {
		if(d.source === d.target) { return drawSelfLoop(d) }
		else { return drawLink(d) }	
    }

    /**
     * Common mousedown event behavior for path and hoverPath
     *
     */
	//Tahiti wichtig
    function mousedownPath(d) {
	if(d3.event.button === 1 || d3.event.button === 2) return;

	if(draggingNode || solveMode) return;

	// select link
	mousedown_link = d;
	selected_node = null;
	hidden_link = d;

	draggingLink = true;
	draggingEntire = true;
	mousedown_node = d.source;
	old_target = d.target;

	var tempTrans = [];
	for(var i = 0; i < alphabet.length; i++)
	    tempTrans[i] = false;

	for(var i = 0; i < links.length; i++){
	    if(mousedown_link.source === links[i].source && mousedown_link.target === links[i].target){
		for(var j = 0; j < alphabet.length; j++){
		    if(links[i].trans[j])
			tempTrans[j] = true;
		}
	    }
	}

	drag_trans = tempTrans;

	var toSplice = links.filter(function(l) {
	    return (l.source === mousedown_node && l.target === old_target && l != d);
	});
	toSplice.map(function(l) {
	    links.splice(links.indexOf(l), 1);
	});

	// displays drag_line
	drag_line
	    .style('marker-end', 'url(#end-arrow)')
	    .classed('hidden', false)
	    .attr('d', 'M' + mousedown_node.x + ',' + mousedown_node.y + 'L' + mousedown_node.x + ',' + mousedown_node.y);
	drag_label
	    .text(function(d) { return makeLabel(drag_trans); })
	    .classed('hidden', false);

	restart();
    }

    /**
     * General behavior for mousedown event
     *
     */
    function mousedown() {

	if(d3.event.button === 1 || d3.event.button === 2) return;
	// prevent I-bar on drag
	//d3.event.preventDefault();

	$('.contextMenu').css('display', 'none');

	// because :active only works in WebKit?
	svg.classed('active', true);
	//Tahiti || choosingTmTransition || choosingTMOperations
	if(draggingNode || mousedown_node || mousedown_link || choosingTMTransition || choosingTMOperations || overTrash || overClear) return;

	if(!solveMode) {
		// insert new node at point
		var point = d3.mouse(this);
		addNode(point[0], point[1]);
	}

	restart();
    }

    /**
     * General behavior for mousemove event
     *
     * In particular:
     *   Updates the position of drag_line when moving transition
     */
    function mousemove() {
	var point = d3.mouse(this);
	mouse_x = point[0];
	mouse_y = point[1];

	if(!showMenu && !config.transition.deterministic) {
	    var restBool = false;
	    for(var i = 0; i < nodes.length; i++){
		if(nodes[i].menu_visible)
		    restBool = true;
		nodes[i].menu_visible = false;
	    }
	    if(restBool){
		restart();
	    }
	}

	// Update stub transitions	
	if(useHoverMenu()) {
		hoverMenu.selectAll('path').attr('d', function(node) {
			var relativePos = {
				x: mouse_x - node.x,
				y: mouse_y - node.y
			}
			var angle = getAngle(relativePos.x, relativePos.y)
			var builder = new dBuilder()
			//builder.MoveTo(node.x, node.y)
			builder.MoveTo(0,0)
			builder.moveTo(polarToPlanar(config.node.radius, angle))
			builder.lineTo(polarToPlanar(15, angle))
			return builder.build()
		})
	}

	if(!mousedown_node) return;

	if(draggingNode) return;

	var dx = d3.mouse(this)[0] - mousedown_node.x;
	var dy = d3.mouse(this)[1] - mousedown_node.y;
	var slope = dy / dx;
	var angle = getAngle(dx, dy);
	var nangle = angle + Math.PI / 2;
	var edgeDeviation = 30;
	var textDev = 20;

	// update drag line
	var x = Math.round(mousedown_node.x + config.node.radius * Math.cos(angle));
	var y = Math.round(mousedown_node.y + config.node.radius * Math.sin(angle));
	drag_line.attr('d', function() {
	    if(hover_node === mousedown_node){
		var x1 = Math.round(80 * Math.cos(angle + Math.PI / 4));
		var y1 = Math.round(80 * Math.sin(angle + Math.PI / 4));
		var x2 = Math.round(80 * Math.cos(angle - Math.PI / 4));
		var y2 = Math.round(80 * Math.sin(angle - Math.PI / 4));
		var x3 = Math.round(6 * Math.cos(angle - Math.PI / 4));
		var y3 = Math.round(6 * Math.sin(angle - Math.PI / 4));
		return 'M' + x + ',' + y + ' c' + x1 + ',' + y1 + ' ' + x2 + ',' + y2 + ' ' + x3 + ',' + y3 + '';
	    }
	    else
		return 'M' + x + ',' + y + 'L' + d3.mouse(this)[0] + ',' + d3.mouse(this)[1];
	});

	drag_label.attr( 'x', function() {
	    if(hover_node === mousedown_node){
		var ax = Math.round(70 * Math.cos(angle + Math.PI / 4));
		var bx = Math.round(70 * Math.cos(angle - Math.PI / 4));
		return x + (ax + bx) / 2;
	    }
	    else{
		if(mousedown_node.x > d3.mouse(this)[0]){
		    textDev = textDev + 7;
		} else{
		    textDev = textDev-15;
		}
		return (mousedown_node.x + d3.mouse(this)[0]) / 2 + Math.cos(nangle) * (textDev - 15); }
	})
	    .attr('y', function() {
		if(hover_node === mousedown_node){
		    var ay = Math.round(70 * Math.sin(angle + Math.PI / 4));
		    var by = Math.round(70 * Math.sin(angle - Math.PI / 4));
		    if(y > mousedown_node.y)
			return y + 10 + (ay + by) / 2;
		    return y + (ay + by) / 2;
		}
		else{
		    if(mousedown_node.x > d3.mouse(this)[0]) {
			textDev = textDev - 22;
		    } else {
			textDev = textDev - 10;
		    }
		    return (mousedown_node.y + d3.mouse(this)[1]) / 2 + Math.sin(nangle) * textDev; }
	    });

	restart();
    }

    /**
     * General behavior for mouseup event
     *
     */
    function mouseup() {
	hidden_link = null;
	svg.classed('ctrl', false);
	// because :active only works in WebKit?
	svg.classed('active', false);

	if(mousedown_node) {
	    if(draggingEntire){
		// add link to graph (update if exists)
		for(var i = 0; i < alphabet.length; i++)
		{
		    if(drag_trans[i]){
			var t = []
			for(var j = 0; j < alphabet.length; j++){
			    t[j] = false;
			}
			t[i] = true;
			var refl = false;
			if(mousedown_node === old_target) {
			    refl = true;
			    mousedown_node.reflexiveNum=drag_trans.length;
			}
			links.push({source: mousedown_node, target: old_target, reflexive: refl, trans: t});
		    }
		}

		links.splice(links.indexOf(mousedown_link), 1);
	    }

	    var t = [];
	    for(var i = 0; i < alphabet.length; i++)
		t[i] = false;
	    drag_trans = t;


	    // hide drag line
	    drag_line
		.classed('hidden', true)
		.style('marker-end', '');
	    drag_label
		.classed('hidden', true);

	    var t = [];
	    for(var i = 0; i < alphabet.length; i++)
		t[i] = false;
	    drag_trans = t;

	}

	// clear mouse event vars
	resetMouseVars();
	draggingLink = false;
	draggingEntire = false;
	draggingNode = false;
	newLink = false;
	linkNums(links);
	restart();
    }

    /**
     * Deletes all transitions involving a given state
     *
     */
    function spliceLinksForNode(node) {
	var toSpliceSource = links.filter(function(l) {
	    return (l.source === node);
	});
	toSpliceSource.map(function(l) {
	    links.splice(links.indexOf(l), 1);
	});
	
	var toSpliceTarget = links.filter(function(l) {
	    return (l.target === node);
	});
	// If transition to deleted node, loops back to source
	// if deterministic, and deletes if nondeterministic
	//Tahiti, braucht Veränderung?
	if(config.transition.deterministic && config.node.label !== 'tm') //Tahiti
	{
	    toSpliceTarget.map(function(l) {
		l.target = l.source;
		l.reflexive = true;
		l.source.reflexiveNum++;
	    });
	}
	else {    
	    toSpliceTarget.map(function(l) {
		links.splice(links.indexOf(l), 1);
	    });
	}
    }
    
    /**
     * Generates the label for a given transition
     *
     */
    function makeLabel(trans) {
    	if(config.transition.labeled === false) return ""
    	else return alphabet.filter( function(element, index, array) { return trans[index] === true } ).join(" ")
    }

    /**
     * Assigns an order to all transitions between common states
     *
     * (e.g. if there is a transition 'a' from state 1 to state 2
     * and a transition 'b' also from state 1 to state 2, one will
     * be given a linkNum of 0, and the other 1)
     *
     * Used to properly spaces labels 
     */
    function linkNums(l) {

	var multipleLinks = l.filter(function(link) {
	    var temp = false;

	    for(var i = 0; i < l.length; i++){
			var transIssue = false;
			for(var j = 0; j < alphabet.length; j++){
			    if(l[i].trans[j] && link.trans[j])
				transIssue = true;
			}

			if(l[i].source === link.source && l[i].target === link.target && l.indexOf(link) < i && transIssue)
			    temp = true;
	    }

	    if(epsilonTrans && link.trans[alphabet.length - 1] && link.source === link.target)
		temp = true;

	    return temp;
	});

	multipleLinks.map(function(link) {
	    l.splice(l.indexOf(link), 1);
	});


	//any links with duplicate source and target get an incremented 'linknum'
	for (var i=0; i<l.length; i++) {
	    var temp = 1;
	    var flat = true;

	    for (var j = 0; j < i; j++) {
		if(l[j].source === l[i].source && l[j].target === l[i].target)
		    temp++;
	    }
	    l[i].linknum = temp;

	    var total = 0;

	    for (var j = 0; j < l.length; j++) {
		if(l[j].target === l[i].source && l[j].source === l[i].target)
		    flat = false;
		if(l[j].source === l[i].source && l[j].target === l[i].target)
		    total++;
	    }

	    l[i].flat = flat;

	    if(total > 1)
		flat = false;
	    if(flat)
		l[i].linknum = 0;

	    l[i].totnum = total;

	    if(l[i].source === l[i].target)
		l[i].source.reflexiveNum = total;
	}
    }

    /**
     * Method for when user begins to drag a node
     *
     */
    function dragstart(d, i) {
	force.stop() // stops the force auto positioning before you start dragging
    }

    /**
     * Method describing behavior of when user is dragging a node
     *
     */
    function dragmove(d, i) {
        var tmp_px = d.px+d3.event.dx;
        var tmp_py = d.py+d3.event.dy;
        var tmp_x = d.x+d3.event.dx;
        var tmp_y = d.y+d3.event.dy;
        
        if(tmp_x < 5 || tmp_x>width-5 || tmp_y < 5 || tmp_y>height-5){
            draggingNode = false;
        }else{
            d.px = tmp_px;
            d.py = tmp_py;
            d.x = tmp_x;
            d.y = tmp_y;//automaton1
        }
        tick();  // this is the key to make it work together with updating both px,py,x,y on d !
    }

    /**
     * Method for when user stops dragging a node
     *
     */
    function dragend(d, i) {
	if(overTrash && !d.initial){
		spliceLinksForNode(d);
		if(config.node.label === 'tm')
			spliceTmOperationListForNode(d); //Tahiti
	    nodes.splice(nodes.indexOf(d), 1);
	    mousedown_node = null;
	    menu_node = null;
	    linkNums(links);
	    restart();
	}

	if(overTrash && d.initial){
	    d.x = 200;
	    d.y = 240;

	    warningText.text('Cannot delete initial node');
	    showWarning = true;
	    restart();
	    showWarning = false;
	}

	tick();
	force.resume();
    }

    /**
     * Calculates the angle given by dx and dy
     *
     */
    function getAngle(dx, dy) {
	var slope = dy / dx;
	var angle = Math.atan(slope);
	if (dy === 0 && dx < 0)
	    angle = 1 * Math.PI;
	else if (dy === 0 && dx >= 0)
	    angle = 0;
	else if (dx === 0 && dy < 0)
	    angle = -1 * Math.PI / 2;
	else if (dx === 0 && dy >= 0)
	    angle = 1 * Math.PI / 2;
	else
	    angle = Math.atan(dy / dx);
	if (dx < 0 && dy != 0)
	    angle = angle + Math.PI;
	return angle;
    }

    /**
     * Adds a node to the interface
     * Does not yet draw the node, this only happens after calling restart()
     */
    function addNode(x, y) {
		// insert new node at point
		var idNum = (function () {
	    		/* Since all id's are in the range [0,nodes.length), the last iteration of the for-loop
			 	 * will return. Earlier iterations may return early */
	    		for(var i = 0; i <= nodes.length; i++) {
					if(!nodes.some(function (node, index, array) { return node.id === i })) {
						return i
					}
				}
	    	})();

		var reflNum = 0;
		//Tahiti: & config.node.label !== 'tm'
		if(config.transition.deterministic & config.node.label !== 'tm') {
		    reflNum = alphabet.length;
		}
		
		// Just push the info about the new node to nodes. Canvas will be updated at the next restart()
		var node = {
			id: idNum,
			left: 0,        //for 'prodaut'
			right: 0,       //for 'prodaut'
			states: [],     //for 'powaut'
			initial: false,
			accepting: false,
			reflexiveNum: reflNum,
			flip: true,
			menu_visible: false,
			owner: 0,
			winningPlayer: -1,
			priority: 0
		};
		node.x = x;
		node.y = y;
		nodes.push(node);
		//TahitiTbd? : & config.node.label !== 'tm' 
		if(config.transition.deterministic & !editing & config.node.label !== 'tm') {
		    for(var i = 0; i < alphabet.length; i++){
				var t = [];
				for(var j = 0; j < alphabet.length; j++){
					t.push(i === j)
				}
				links.push({source: node, target: node, reflexive: true, trans: t});
		    }

		    linkNums(links);
		}
		
		return idNum;
    }

    /**
     * Local function to initializes the interface
     *
     */
    function init(){
    	if(config.hasInitialNode) {
			initial_node = null;
		}
	nodes = [];
	links = [];
	nodes.length = 0;
	links.length = 0;

	resetMouseVars();

	restart();

	addNode(200, 240);
	nodes[0].initial = true;
	
	if(config.hasInitialNode) {
		initial_node = nodes[0];
	}

	restart();
    }
    
    /**
     * Describes the right click menu for interface
     *
     */
    $(container).contextMenu(
    	{menu: 'cmenu_canvas', inSpeed: 200, outSpeed: 300},
		function(action, el, pos, evt) {
		    switch ( action ) {
		    case 'add':
				addNode(mouse_x, mouse_y);
				restart();
				break;
		    case 'remove':
				spliceLinksForNode(menu_node);
				if(config.node.label === 'tm')
					spliceTmOperationListForNode(menu_node);
				nodes.splice(nodes.indexOf(menu_node), 1);
				hidden_link = null;
				linkNums(links);
				restart();
				break;
		    case 'final':
		    case 'non-final':
				menu_node.accepting = !menu_node.accepting;
				restart();
				break;
		    case 'init':
				for(var i = 0; i < nodes.length; i++) {
				    nodes[i].initial = false;
				}
				menu_node.initial = true;
				initial_node = menu_node;
				restart();
				break;
		    case 'flip':
				menu_node.flip = !menu_node.flip;
				restart();
				break;
		    case 'flip_edge':
				menu_link.source.flip = !menu_link.source.flip;
				restart();
				break;
		    case 'remove_edge':
				var toSplice = links.filter( function(l) {
				    return (l.source === menu_link.source && l.target === menu_link.target);
				});

				toSplice.map(function(l) {
				    links.splice(links.indexOf(l), 1);
				});
				restart();
				break;
		    case 'remove_edge_label':
				links.splice(links.indexOf(menu_link), 1);
				restart();
				break;
			case 'makep0':
				menu_node.owner = 0;
				restart();
				break;
			case 'makep1':
				menu_node.owner = 1;
				restart();
				break;
			case 'p0wins':
				menu_node.winningPlayer = 0;
				restart();
				break;
			case 'p1wins':
				menu_node.winningPlayer = 1;
				restart();
				break;
			case 'changePriority':
				var newPriority = parseInt(prompt("Please enter the new priority", menu_node.priority))
				if(newPriority !== NaN && newPriority >= 0) { menu_node.priority = newPriority; restart(); }
				break;
		    default:
				break;
		    }
		},
		function(e) {
		    var menu_items = $('#cmenu_canvas > li');
		    menu_items.disableContextMenu();
			if(solveMode) return

		    if(hover_node){
				if(hover_node.accepting) {
					menu_items.enableContextMenuItems('#non-final');
				} else {
					menu_items.enableContextMenuItems('#final');
				}

				if(hover_node.reflexiveNum > 0) {
					menu_items.enableContextMenuItems('#flip')
				}

				if(config.hasInitialNode && !(hover_node.initial)) {
					menu_items.enableContextMenuItems('#remove')
					menu_items.enableContextMenuItems('#init')
				} else if (!config.hasInitialNode) {
					menu_items.enableContextMenuItems('#remove')
				}

				if(config.twoPlayers === true) {
					if(hover_node.owner === 0) {
						menu_items.enableContextMenuItems('#makep1')
					} else {
						menu_items.enableContextMenuItems('#makep0')
					}
					/*
					if(hover_node.winningPlayer === 0) {
						menu_items.enableContextMenuItems('#p1wins')
					} else {
						menu_items.enableContextMenuItems('#p0wins')
					}*/
				}

				if(config.node.label === 'priority') {
					menu_items.enableContextMenuItems('#changePriority')
				}
		    } else if (hover_link) {
				if(hover_label && !config.transition.deterministic && hover_link.reflexive) {
				    menu_items.enableContextMenuItems('#remove_edge_label,#flip_edge');
				} else if(hover_label && !config.transition.deterministic) {
				    menu_items.enableContextMenuItems('#remove_edge_label');
				}  else if(hover_link.reflexive && !config.transition.deterministic) {
				    menu_items.enableContextMenuItems('#remove_edge,#flip_edge');
				} else if (hover_link.reflexive && config.transition.deterministic) {
				    menu_items.enableContextMenuItems('#flip_edge');
				} else if (!config.transition.deterministic) {
				    menu_items.enableContextMenuItems('#remove_edge');
				}
		    } else {
				menu_items.enableContextMenuItems('#add');
		    }
		});

    /**
     * Public function to initialize the interface
     *
     */
    this.initialize = function() {
	
	if(!started){
	    force = d3.layout.force()
		.nodes(nodes)
		.links(links)
		.size([width, height])
		.on('tick', tick);

	    node_drag = d3.behavior.drag()
		.on("dragstart", dragstart)
		.on("drag", dragmove)
		.on("dragend", dragend);

	    svg.on('mousedown', mousedown)
		.on('mousemove', mousemove)
		.on('mouseup', mouseup);

	    started = true;
	}
	
	this.clear();
	addNode(200, 240);
	
	if(config.hasInitialNode) {
		nodes[0].initial = true;
	 	initial_node = nodes[0];
	 }
	
	restart();
    }

    editing = false;
    /**
     * Draws automaton described by xml
     *
     */
	//Tahiti loadAutomaton
    this.setAutomaton = function (xml) {
        editing = true;
		if(!started) {
		    this.initialize();
		}

		var xmlDoc = Utils.text2xml(xml);
		var alph = xmlDoc.getElementsByTagName("alphabet")[0];
		var symbolTags = alph.getElementsByTagName("symbol");
		var symbols = new Array();
		for (i = 0; i < symbolTags.length; i++) {
		    symbols.push(symbolTags[i].firstChild.nodeValue.trim());
		}
		if (config.node.label === 'tm'){
			var tapeDats = xmlDoc.getElementsByTagName("tmData")[0];
			numberOfTapes = parseInt(tapeDats.getElementsByTagName("tapeNumber")[0].firstChild.nodeValue.trim());
			tmModeIsFunction = (tapeDats.getElementsByTagName("tmFunction")[0].firstChild.nodeValue.trim() === "true");
			//console.log(numberOfTapes + " numberOfTapes!")
		}
		this.setAlphabet(symbols);
		if(config.node.label === 'tm')
			this.setNumberOfTapes(numberOfTapes);
		initial_node = null;
		nodes = [];
		links = [];
		nodes.length = 0;
		links.length = 0;
		resetMouseVars();
		restart();

		var stateTags = xmlDoc.getElementsByTagName("stateSet")[0].getElementsByTagName("state");
		for (i = 0; i < stateTags.length; i++) {
		    var currState = stateTags[i];
		    var posX = parseFloat(currState.getElementsByTagName("posX")[0].firstChild.nodeValue) * width / 1000;
		    var posY = parseFloat(currState.getElementsByTagName("posY")[0].firstChild.nodeValue) * height / 1000;
		    var nodeId = parseInt(currState.getElementsByTagName("label")[0].firstChild.nodeValue);
		    addNode(posX, posY);
		    nodes[nodes.length - 1].id = nodeId;
		    if(config.twoPlayers) {
				nodes[nodes.length - 1].owner = parseInt(currState.getAttribute("owner"))
				nodes[nodes.length - 1].winningPlayer = parseInt(currState.getAttribute("winner"))
			}
			if(config.node.label === 'priority') {
				nodes[nodes.length - 1].priority = parseInt(currState.getAttribute("priority"))
			}
		}

		//TODO merge edges that have the same source+target and concat the labels
		var edgeTags = xmlDoc.getElementsByTagName("transitionSet")[0].getElementsByTagName("transition");
		for (i = 0; i < edgeTags.length; i++) {
		    var currEdge = edgeTags[i];
		    var from = parseInt(currEdge.getElementsByTagName("from")[0].firstChild.nodeValue);
		    var to = parseInt(currEdge.getElementsByTagName("to")[0].firstChild.nodeValue);
		    if(currEdge.getElementsByTagName("read")[0].firstChild) {
		    	var read = currEdge.getElementsByTagName("read")[0].firstChild.nodeValue.trim();
			} else {
				var read = ""
			}
		    
		    var transArray = [];
		    for(var j = 0; j < alphabet.length; j++){
		    	transArray.push(alphabet[j] === read)
		    }

		    var fromNodeIndex = nodes.findIndex(function (node) { return node.id == from});
		    var fromNode = nodes[fromNodeIndex];
		    
		    var toNodeIndex = nodes.findIndex(function (node) { return node.id == to});
		    var toNode = nodes[toNodeIndex];

		    var refl = (fromNode === toNode);

			links.push({source: fromNode, target: toNode, trans: transArray, reflexive: refl});
			if(config.node.label === 'tm'){
				var write = currEdge.getElementsByTagName("write")[0].firstChild.nodeValue.trim();
				var head = currEdge.getElementsByTagName("head")[0].firstChild.nodeValue.trim();
				//console.log("ik will eine operation adden mit " + write + " und " + head);
				addTmOperationToList(fromNode, toNode, makeLabel(transArray), write, head);
			}
		}
		//console.log(tmOperationList);

		var accTags = xmlDoc.getElementsByTagName("acceptingSet")[0].getElementsByTagName("state");
		for (i = 0; i < accTags.length; i++) {
		    var nodeId = parseInt(accTags[i].getAttribute('sid'));
		    
		    for(var j = 0; j < nodes.length; j++){
			if(nodes[j].id === nodeId)
			    nodes[j].accepting = true;
		    }
		}

		if(config.hasInitialNode) {
			var initTag = xmlDoc.getElementsByTagName("initState")[0].getElementsByTagName("state");
			var initId = parseInt(initTag[0].getAttribute('sid'));
			initial_node = nodes[0];
			for(i = 0; i < nodes.length; i++) {
			    if(nodes[i].id === initId){
					nodes[i].initial = true;
					initial_node = nodes[i];
			    }
			}
		}

		restart();
        editing = false;
    }

    /**
     * Compiles xml describing alphabet of automaton
     *
     */
    this.exportAlphabet = function () {
		//Alphabet Tahiti export
		if(config.node.label === 'tm'){
			var alpha = "	<alphabet>\n";
			for (var i = 0; i < alphabetSingle.length; i++){
				alpha = alpha + " <symbol>" + alphabetSingle[i] + "</symbol>\n";
			}
			alpha = alpha + "	</alphabet>\n";
			//window.alert("alphabet: " + alpha);
			return alpha;
		}
		var alpha = "	<alphabet>\n";
		for (var i = 0; i < alphabet.length; i++){
		    alpha = alpha + " <symbol>" + alphabet[i] + "</symbol>\n";
		}
		//window.alert("fullAlphabet:");
		alpha = alpha + "	</alphabet>\n";

		return alpha;
    }

    /**
     * Compiles xml describing automatonHint 
     *
     */
    this.exportAutomatonHint = function () {
		var aut = this.exportAutomaton();
		var level = "<level>" + $('input[name=feedlev]:radio:checked').val() + "</level>\n";
		var metrics = "<metrics>" + $('input[name=enabFeed]:checkbox:checked').map(function (value, index) { return value; }).get().join(",") + "</metrics>\n"

		return "<automatonHint>\n" + aut + level + metrics + "</automatonHint>";
    }

    //export a simple text version
    this.exportAutomaton = function () {
		//console.log("Achtung, automat wird exportiert");
		//Alphabet
		var alpha = this.exportAlphabet();

		//States
		var states = "	<stateSet>\n";

		var accepting = new Array()
		var init = false
		if(config.hasInitialNode) {
			var initState = "<initState><state sid='" + initial_node.id + "' /></initState>";
		} else {
			var initState = ""
		}

		for(var i = 0; i < nodes.length; i++){
			if(nodes[i].accepting){
				accepting.push(nodes[i].id);
			}

			states = states + "		<state sid='" + nodes[i].id + "' "
			if(config.twoPlayers) {
				states += "owner='" + nodes[i].owner + "' "
				states += "winner='" + nodes[i].winningPlayer + "' "
			}
			if(config.node.label === 'priority') {
				states += "priority='" + nodes[i].priority + "' "
			}
			states += "><id>" + nodes[i].id + "</id>";
			if(config.node.label === 'tuple') {
			    states += "<label>" + nodes[i].left + "," + nodes[i].right + "</label>"
			}
			else if(config.node.label === 'set') {
			    states += "<label>"
			    var last = -1
			    for(var j = 0; j < nodes[i].states.length; j++) {
			        if(nodes[i].states[j]) {
			            last = j;
			        }
			    }
			    if(last > -1) {
			        for(var j = 0; j < last; j++) {
			            if(nodes[i].states[j]) {
			                states += j + ","
			            }
			        }
			        if(nodes[i].states[last]) {
			            states += (last)
			        }
			    }
			    states += "</label>"
			}
			else {
			    states += "<label>" + nodes[i].id + "</label>"
			}

            states += "<posX>" + Math.round(parseFloat(nodes[i].x) * 1000 / width) + "</posX><posY>" + Math.round(parseFloat(nodes[i].y) * 1000 / height) + "</posY></state>\n";

		}
		states = states + "	</stateSet>\n";

		// Transitions
		var transitions = "	<transitionSet>\n";

		var transitionNo = 0;

		//Tahiti TmLinks
		if(config.node.label === 'tm'){
			for(var i = 0; i < links.length; i++){
				if(!links[i].hidden){
					var fromId = links[i].source.id;
					var toId = links[i].target.id;
					var edgeDistance = 30 + "";
					var labels = [];

					for(var j = 0; j < alphabet.length; j++){
						if(links[i].trans[j])
							labels.push(alphabet[j]);
					}

					for(var j = 0; j < labels.length; j++) {
						var linkOperations = getTmOperations(links[i].source, links[i].target, labels[j]);
						transitions = transitions + "		<transition tid='" + transitionNo + "'>\n"
						+ "			<from>" + fromId + "</from>\n"
						+ "			<to>" + toId + "</to>\n"
						+ "			<read>" + labels[j] + "</read>\n"
						+ "			<edgeDistance>" + edgeDistance + "</edgeDistance>\n"
						+ " 		<write>" + linkOperations[0] + "</write>\n"
						+ " 		<head>" + linkOperations[1] + "</head>\n"
						+ "		</transition>\n";
						transitionNo = transitionNo + 1;
					}
					if(labels.length == 0) {
						transitions += "<transition tid='" + transitionNo + "'>\n"
						+ "			<from>" + fromId + "</from>\n"
						+ "			<to>" + toId + "</to>\n"
						+ "			<read></read>\n"
						+ "			<edgeDistance>" + edgeDistance + "</edgeDistance>\n"
						+ "		</transition>\n";
						transitionNo = transitionNo + 1;
					}
				}
			}
		}
		else{
			for(var i = 0; i < links.length; i++){
				if(!links[i].hidden){
					var fromId = links[i].source.id;
					var toId = links[i].target.id;
					var edgeDistance = 30 + "";
					var labels = [];

					for(var j = 0; j < alphabet.length; j++){
						if(links[i].trans[j])
							labels.push(alphabet[j]);
					}

					for(var j = 0; j < labels.length; j++) {
						transitions = transitions + "		<transition tid='" + transitionNo + "'>\n"
						+ "			<from>" + fromId + "</from>\n"
						+ "			<to>" + toId + "</to>\n"
						+ "			<read>" + labels[j] + "</read>\n"
						+ "			<edgeDistance>" + edgeDistance + "</edgeDistance>\n"
						+ "		</transition>\n";
						transitionNo = transitionNo + 1;
					}
					if(labels.length == 0) {
						transitions += "<transition tid='" + transitionNo + "'>\n"
						+ "			<from>" + fromId + "</from>\n"
						+ "			<to>" + toId + "</to>\n"
						+ "			<read></read>\n"
						+ "			<edgeDistance>" + edgeDistance + "</edgeDistance>\n"
						+ "		</transition>\n";
						transitionNo = transitionNo + 1;
					}
				}
			}
		}
		transitions = transitions + "	</transitionSet>\n";

		var acc = "	<acceptingSet>\n"
		for (var i = 0; i < accepting.length; i++) {
			acc = acc + "		<state sid='" + accepting[i] + "'/>\n"
		}
		acc = acc + "	</acceptingSet>\n"

		//Tahiti saveTapeNumber
		var tmData = "";
		if (config.node.label === 'tm'){
			tmData = "	<tmData>\n"
			tmData = tmData + "		<tapeNumber>" + numberOfTapes + "</tapeNumber>\n"
			tmData = tmData + "		<tmFunction>" + tmModeIsFunction + "</tmFunction>\n"
			tmData = tmData + "	</tmData>\n"
		}

		var ret = "<automaton>\n" + alpha + states + transitions + acc + tmData + initState + "</automaton>\n";
		//console.log(ret);
		return ret;
	}

    /**
     * Clears the interface
     *
     */
     this.clear = function () {
     	nodes = [];
     	links = [];
     	nodes.length = 0;
     	links.length = 0;
     	initial_node = null;

     	resetMouseVars();

     	restart();
     }

    /**
     * Sets the alphabet
     *
     */
	//Tahiti alphabet
     this.setAlphabet = function (alph) {
		 //window.alert("alph: " + alph);
     	alphabet = alph;
		 if(config.node.label === 'tm'){
			 alphabetSingle = alph;
			 //if(alphabet[0].length !== numberOfTapes){
				var newAlphabet = []; //new Array(alphabetSingle.length * alphabetSingle.length); 
				//for(var i = 0; i < (alphabetSingle.length ^ alphabetSingle.length); i++){
				//numberOfTapes = parseInt(numberOfTapes);
				this.createTMAlphabet(newAlphabet, []);
				//}
				alphabet = newAlphabet;
			 //}
		}
     	if(epsilonTrans && !config.transition.deterministic){
     		alphabet.push('\u03B5');
     	}

     	this.initialize();
	 }
	  
	 this.createTMAlphabet = function (newAlphabet, word) {
		if(word.length === numberOfTapes){
			 newAlphabet.push(word);
			 return;
		}			 			 
		//var returnWord = new Array(numberOfTapes);
		for(var i = 0; i < alphabetSingle.length; i++){
			this.createTMAlphabet(newAlphabet, word + alphabetSingle[i]);
			//console.log("newAlphabet: " + newAlphabet);
		}
	 }

     /**
      * Sets the number of states of the automata to construct product from
      *
      */
     this.setNumberOfStates = function(xml1, xml2) {

        var xml1Doc = Utils.text2xml(xml1);
        var xml2Doc = Utils.text2xml(xml2);
        var stateTags1 = xml1Doc.getElementsByTagName("stateSet")[0].getElementsByTagName("state");
        var stateTags2 = xml2Doc.getElementsByTagName("stateSet")[0].getElementsByTagName("state");

        numberOfNodesOfAutomaton1 = stateTags1.length
        numberOfNodesOfAutomaton2 = stateTags2.length

        this.initialize();
        this.restart();

     }

    /**
     * Sets whether or not to use epsilon transitions
     *
     */
     this.setEpsilon = function(b) {
     	epsilonTrans = b;

     	this.setAlphabet(alphabet);
	 }
	//Tahiti setTmMode 
	this.setTmMode = function(b) {
		tmModeIsFunction = b;
		//this.setAlphabet(alphabet);
	}
	this.setNumberOfTapes = function(b) {
		//window.alert("Number: " + b);
		numberOfTapes = b;
		this.initializeTmOperationList();
	}

	this.getStepMode = function() {
		return stepByStep;
	}

	this.setStepMode = function(n) {
		stepByStep = n;
	}

	this.resetTapes = function() {
		firstViewableTapeElement = [];
		for (var i = 0; i < numberOfTapes; i++)
			firstViewableTapeElement.push(50);
		//console.log("tapelist: " + tapeList + " initTapeWords: " + initTapeWords);
		//tapeList = initTapeWords;
		this.loadWordsIntoTapes(initTapeWords);
		//this.fillTMTapes();
	}

	this.resetCurrentNode = function() {
		currentNode = initial_node;
		restart();
	}
	
	this.initializeTmOperationList = function(b){
		tmOperationList = [];
		initialTmWriteOperation = "";
		for(var i = 0; i < numberOfTapes; i++)
			initialTmWriteOperation = initialTmWriteOperation + alphabetSingle[0];
		initialTmHeadOperation = "";
		for(var i = 0; i < numberOfTapes; i++)
			initialTmHeadOperation = initialTmHeadOperation + "R";
	}
	//Tahiti create tmTapes
	this.createTMTapes = function(b) {
		numberOfTapes = b;
		tapeList = new Array(numberOfTapes);
		tapeList.push([])
		//tapeList = [];
		//for (var i = 0; i < numberOfTapes; i++){
		//	tapeList.push([0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15])
		//}
		//window.alert("hiho " + numberOfTapes);
		//tapeList = [0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15];
		//window.alert("hiho");

		//var tapes = svg.append('svg:rect')
		//	.attr("x", 10)
		//	.attr("y", 10)
		//	.attr("width", 50)
		//	.attr("height", 100);
		
		//var tapeElements = svg.selectAll("g")
		//.data(tapeList)
		//.enter().append("g")
		//.attr("transform", function(d, i) { return "translate(" + 10 + (tapeElementWidth*i) + "," + tapeElementHeight + ")"; });
		
		//tapeElements.append("rect")
		//.attr("width", tapeElementWidth)
		//.attr("height", tapeElementHeight);
	
		//tapeElements.append("text")
		//.attr("x", function(d) { return x(d) - 3; })
		//.attr("y", tapeElementHeight / 2)
		//.attr("dy", ".35em")
		//.text(function(d) { return d; });

		/*var tapes = svg.selectAll("rect")
			.data(tapeList)
			.enter()
			.append("rect")
		*/
		for (var i = 0; i < numberOfTapes; i++){
			firstViewableTapeElement.push(50);
			//var tapes = svg.selectAll("rect")
			//	.data(tapeList[i])
			//	.enter()
			//	.append("rect")
			//window.alert("tapelist: " + tapeList[i]);
			for (var k = 0; k < 3; k++){
				var dots = svg.append('svg:text')
					.attr("x", 3 + (k * 3))
					.attr("y", 10 + ((10+tapeElementHeight)*i) + (tapeElementHeight/2))
					.attr("dy", ".35em")
					.text(".");
			}

			for (var k = 0; k < 3; k++){
				var dots = svg.append('svg:text')
					.attr("x", 17 + (tapeElementsPerLine * tapeElementWidth) + (k * 3))
					.attr("y", 10 + ((10+tapeElementHeight)*i) + (tapeElementHeight/2))
					.attr("dy", ".35em")
					.text(".");
			}

			for (var j = 0; j < tapeElementsPerLine; j++){
				var tapeElement = svg.append('svg:rect')
					.attr("x", 15 + (j*tapeElementWidth))
					.attr("y", 15 + ((10+tapeElementHeight)*i))
					.attr("width", tapeElementWidth)
					.attr("height", tapeElementHeight)
					.style('stroke', '#5B90B2')
					.style('fill-opacity', '0.0')
					.style('stroke-width', '1px')

				//var tapeElementLabel = svg.append("text")
				//	.attr("x", 10 + (j*tapeElementWidth))
				//	.attr("y", 10 + ((5+tapeElementHeight)*i))
				//	.attr("dy", ".35em")
				//	.text(j);
			}
			/*var tapeAttributes = tapes
				.attr("x", function (d) { return (10 + (d*tapeElementWidth)); })
				.attr("y", 10 + ((5+tapeElementHeight)*i))
				.attr("width", tapeElementWidth)
				.attr("height", tapeElementHeight)
				.style('stroke', '#5B90B2')
				.style('fill-opacity', '0.0')
				.style('stroke-width', '1px')
				*/
			var markedElement = svg.append('svg:rect')
				.attr("x", 15 + tapeElementWidth*tapeHeadOffset)
				.attr("y", 15 + ((10+tapeElementHeight)*i))
				.attr("width", tapeElementWidth)
				.attr("height", tapeElementHeight)
				.style('stroke', '#5B90B2')
				.style('fill-opacity', '0.0')
				.style('stroke-width', '3px')
		}

		for(var i = 0; i < numberOfTapes; i++){
			var tempList = [];
			for (var j = 0; j < 250; j++){
				//window.alert("tapelist: " + tapeList + " element " + i + " is " + tapeList[i]);
				tempList.push("\u25FB");
			}
			//window.alert(tempList);
			tapeList[i] = tempList;
			//window.alert(tapeList);
		}
		trashLabel.style('visibility', 'hidden');
		clearRect.style('visibility', 'hidden');
		clearText.style('visibility', 'hidden');
		/*b = new Array(numberOfTapes);
		b[0] = ['q','w','e','r','t','z','u'];
		b[1] = ['a','b','c'];
		b[2] = ['x','x','x'];
		this.loadWordsIntoTapes(b);
		this.fillTMTapes();*/
		this.fillTMTapes();
	}

	this.setExampleTapeWord = function(b) {
		//window.alert("setExampleTapeWord( " + b + " )");
		if (b === "" || b === null)
			return;
		tapeExample = new Array(numberOfTapes);
		//console.log(b)
		b = b.split("\n")
		//console.log(b)
		b = b[0].split("->")
		//console.log(b)
		b = b[0].split(",")
		//console.log(b)
		for(var i = 0; i < numberOfTapes; i++)
			tapeExample[i] = b[i];
		//window.alert(tapeExample);
		this.loadWordsIntoTapes(tapeExample);
	}

	this.initializeStepByStepCanvas = function(b){
		if(stepByStep === 0){
			currentNode = initial_node;
			stepByStep = 1;
			//console.log("stepByStep ist jetzt " + stepByStep);
			restart();
		}
	}

	this.initializeStepByStepCanvasTapes = function(b){
		//console.log(b);
		initTapeWords = b;
		this.loadWordsIntoTapes(b);
	}

	this.tmOperationStepTape = function(writeAndMove){
		//console.log(writeAndMove);
		if(writeAndMove === null){
			window.alert("No matching transitions");
			return;
		}
		for(var i = 0; i < writeAndMove[0].length; i++){
			tapeList[i][firstViewableTapeElement[i]+tapeHeadOffset] = writeAndMove[0][i];
			if(writeAndMove[1][i] === 'R')
				firstViewableTapeElement[i]++;
			if(writeAndMove[1][i] === 'L')
				firstViewableTapeElement[i]--;
		}
		this.fillTMTapes();
	}

	this.getSymbolsOnTapes = function(){
		returnString = "";
		for(var i = 0; i < tapeList.length; i++)
			returnString = returnString + tapeList[i][firstViewableTapeElement[i]+tapeHeadOffset];
		return returnString;
	}

	this.loadWordsIntoTapes = function(b) {
		//var headPosition = firstViewableTapeElement+tapeHeadOffset;
		//window.alert(headPosition);
		tapeList = new Array(numberOfTapes);
		tapeList.push([]);
		for(var i = 0; i < numberOfTapes; i++){
			var tempList = [];
			for (var j = 0; j < 250; j++){
				//window.alert("tapelist: " + tapeList + " element " + i + " is " + tapeList[i]);
				tempList.push("\u25FB");
			}
			//window.alert(tempList);
			tapeList[i] = tempList;
			//window.alert(tapeList);
		}
		for(var i = 0; i < b.length; i++){
			for(var j = 0; j < b[i].length; j++){
				//inser = headPosition+j
				//window.alert(tapeList[i][headPosition[i]+j]);
				//window.alert(b);
				//console.log(numberOfTapes + "   " + tapeList[i] + "   " + b[i][j]);
				tapeList[i][firstViewableTapeElement[i]+tapeHeadOffset+j] = b[i][j];
			}
		}
		//initTapeWords = tapeList;
		this.fillTMTapes();
	}

	function addInitialTmOperationToList(source, target, symbolsToRead){
		newTmLink = true;
		for(var i = 0; i < tmOperationList.length; i++)
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){ //&& nodeHasOutgoingTmOperation(source, ))
				tmOperationList[i][2].push(symbolsToRead);
				tmOperationList[i][3].push(initialTmWriteOperation);
				tmOperationList[i][4].push(initialTmHeadOperation);
				newTmLink = false;
				break;
			}
		if(newTmLink)
			tmOperationList.push([source, target, [symbolsToRead], [initialTmWriteOperation], [initialTmHeadOperation]]);
		//for(var j = 0; j < tmOperationList[i][1])
		//window.alert(tmOperationList + " Anzahl Einträge: " + tmOperationList.length);
	}

	function getTmOperations(source, target, symbolsToRead){
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){
				for(var j = 0; j < tmOperationList[i][2].length; j++){
					if(tmOperationList[i][2][j] === symbolsToRead){
						//console.log(tmOperationList[i] + " hier fehler?")
						return [tmOperationList[i][3][j],tmOperationList[i][4][j]];
					}
				}
			}
		}
	}

	function spliceTmOperationListForNode(d){
		var iList = [];
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === d || tmOperationList[i][1] === d){
				iList[iList.length] = i;
			}
		}
		for(var i = iList.length; i > 0; i--){
			tmOperationList.splice(iList[i-1], 1);
			//console.log("Dies ist eine entfernte Operation: " + tmOperationList.splice(iList[i-1], 1));
		}
	}

	function removeTmOperationFromList(source, target, symbolsToRead){
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){
				//console.log("ik hab diesen link gefunden: " + tmOperationList[i]);
				//console.log("tmOperationList[i][2]: " + tmOperationList[i][2] + " mit länge " + tmOperationList[i][2].length);
				for(var j = 0; j < tmOperationList[i][2].length; j++){
					//console.log("Ist " + tmOperationList[i][2][j] + " ueberhaupt " + hover_label_label.text());
					//console.log(hover_label_label.text());
					if(tmOperationList[i][2][j] === symbolsToRead){
						//console.log("punshkin: " + tmOperationList[i][2][j]);
						tmOperationList[i][2].splice(j, 1);
						tmOperationList[i][3].splice(j, 1); //= hover_label_label.attr("writeOp");
						tmOperationList[i][4].splice(j, 1); //= hover_label_label.attr("headOp");
						if(tmOperationList[i][2].length === 0)
							removeEntireTmOperationFromList(source, target);
						break;
					}
				}
			}
		}
	}

	function removeEntireTmOperationFromList(source, target){
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){
				tmOperationList.splice(i, 1);
			}
		}
		//console.log("The new OperationList: " + tmOperationList);
	}

	function changeTmOperationInList(){
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === transitionToEdit.source && tmOperationList[i][1] === transitionToEdit.target){
				for(var j = 0; j < tmOperationList[i][2].length; j++){
					if(tmOperationList[i][2][j] === hover_label_label.text()){
						tmOperationList[i][3][j] = hover_label_label.attr("writeOp");
						tmOperationList[i][4][j] = hover_label_label.attr("headOp");
						operationTextDisplay.text(getVisualTmOperationFromLabel(hover_label_label));
					}
				}
			}
		}
	}
	//Tbd!
	function changeTmOperationTarget(source, target, symbolsToRead, newTarget){
		//console.log("ik werde aufgerufen mit: " + source + "," + target + "," + symbolsToRead + "," + newTarget);
		var newTmOperation = null;
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){
				//console.log("ik hab diesen link gefunden: " + tmOperationList[i]);
				for(var j = 0; j < tmOperationList[i][2].length; j++){
					if(tmOperationList[i][2][j] === symbolsToRead)
						newTmOperation = [source, newTarget, tmOperationList[i][2][j], tmOperationList[i][3][j], tmOperationList[i][4][j]];
				}
			}
		}
		if(newTmOperation !== null){
			removeTmOperationFromList(source, target, symbolsToRead);
			addTmOperationToList(newTmOperation[0], newTmOperation[1], newTmOperation[2], newTmOperation[3], newTmOperation[4]);
		}
	}

	function changeEntireTmOperationTarget(source, target, newTarget){
		var newTmOperation = null;
		for(var i = 0; i < tmOperationList.length; i++){
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){
				newTmOperation = tmOperationList[i];
				break;
			}
		}
		if(newTmOperation !== null){
			removeEntireTmOperationFromList(source, target);
			newTmOperation[1] = newTarget;
			for(var i = 0; i < newTmOperation[2].length; i++){
				addTmOperationToList(newTmOperation[0], newTmOperation[1], newTmOperation[2][i], newTmOperation[3][i], newTmOperation[4][i]);
			}
			//tmOperationList.push(newTmOperation);//addTmOperationToList(newTmOperation);
		}
	}
	///TAHITI LESEZEICHEN
	function addTmOperationToList(source, target, symbolsToRead, write, head){
		newTmLink = true;
		for(var i = 0; i < tmOperationList.length; i++)
			if(tmOperationList[i][0] === source && tmOperationList[i][1] === target){
				tmOperationList[i][2].push(symbolsToRead);
				tmOperationList[i][3].push(write);
				tmOperationList[i][4].push(head);
				newTmLink = false;
				break;
			}
		if(newTmLink)
			tmOperationList.push([source, target, [symbolsToRead], [write], [head]]);
		//console.log("hinzugefügt ergibt: " + tmOperationList);
	}

	function getVisualTmOperationFromLabel(l){
		var returnString = l.text()[0] + "/" + l.attr('writeOp')[0] + "," + l.attr('headOp')[0];
		for(var i = 1; i < l.text().length; i++)
			returnString = returnString + " | " + l.text()[i] + "/" + l.attr('writeOp')[i] + "," + l.attr('headOp')[i];
		return returnString;
	}

	function getVisualTmOperationFromValues(read, write, head){
		var returnString = read[0] + "/" + write[0] + "," + head[0];
		for(var i = 1; i < read.length; i++)
			returnString = returnString + " | " + read[i] + "/" + write[i] + "," + head[i];
		return returnString;
	}

	//function fillTmOperationList(){
	//	tmOperationList = [];
	//}

	/*this.tmOperationStep = function(symbolsOnTape) {
		for(var i = 0; i < tmOperationList.length; i++){
			if(currentNode === tmOperationList[i][0])
				outgoingTransitions[outgoingTransitions.length] = tmOperationList[i];
		}
	}*/

	this.tmOperationStep = function(symbolsOnTape) {
		//tmOperationList -> [[Start-Node, Ziel-Node, Lesezeichen-["","",""], Schreibzeichen-["","",""], Kopfbewegungen-["","",""]]
		var operationFound = false;
		var outgoingTransitions = [];
		//console.log(symbolsOnTape);
		//console.log(symbolsOnTape[0] + " " + symbolsOnTape[1]);
		for(var i = 0; i < tmOperationList.length; i++){
			if(currentNode === tmOperationList[i][0])
				outgoingTransitions[outgoingTransitions.length] = tmOperationList[i];
		}
		//console.log(outgoingTransitions);
		var i = 0;
		var k = 0;
		var j = 0;
		for(i = 0; i < outgoingTransitions.length; i++){
			for(k = 0; k < outgoingTransitions[i][2].length; k++){
				operationFound = true;
				for(j = 0; j < numberOfTapes; j++){
					//console.log("is " + symbolsOnTape[j] + " === " + outgoingTransitions[i][2][k][j]) + " ?";
					if (!(symbolsOnTape[j] === outgoingTransitions[i][2][k][j])){
						operationFound = false;
						//console.log(operationFound);
					}
				}
				if (operationFound === true)
					break;
			}
			if (operationFound === true)
				break;
		}
		if (operationFound === true){
			//console.log("wahrheit");
			currentNode = outgoingTransitions[i][1];
			//console.log("return value: " + [outgoingTransitions[i][3][k], outgoingTransitions[i][4][k]]);
			restart();
			return [outgoingTransitions[i][3][k], outgoingTransitions[i][4][k]];
		} else {
			return null;
			//turingmachine stops
		}
	}

	this.getNextTmOperation = function(symbolsOnTape) {
		//tmOperationList -> [[Start-Node, Ziel-Node, Lesezeichen-["","",""], Schreibzeichen-["","",""], Kopfbewegungen-["","",""]]
		var operationFound = false;
		var outgoingTransitions = [];
		//console.log(symbolsOnTape);
		//console.log(symbolsOnTape[0] + " " + symbolsOnTape[1]);
		for(var i = 0; i < tmOperationList.length; i++){
			if(currentNode === tmOperationList[i][0])
				outgoingTransitions[outgoingTransitions.length] = tmOperationList[i];
		}
		//console.log(outgoingTransitions);
		var i = 0;
		var k = 0;
		var j = 0;
		for(i = 0; i < outgoingTransitions.length; i++){
			for(k = 0; k < outgoingTransitions[i][2].length; k++){
				operationFound = true;
				for(j = 0; j < numberOfTapes; j++){
					//console.log("is " + symbolsOnTape[j] + " === " + outgoingTransitions[i][2][k][j]) + " ?";
					if (!(symbolsOnTape[j] === outgoingTransitions[i][2][k][j])){
						operationFound = false;
						//console.log(operationFound);
					}
				}
				if (operationFound === true)
					break;
			}
			if (operationFound === true)
				break;
		}
		if (operationFound === true){
			//console.log("wahrheit");
			//currentNode = outgoingTransitions[i][1];
			//console.log("return value: " + [outgoingTransitions[i][3][k], outgoingTransitions[i][4][k]]);
			return "Next Step: " + getVisualTmOperationFromValues(outgoingTransitions[i][2][k], outgoingTransitions[i][3][k], outgoingTransitions[i][4][k]);
		} else {
			return "No Matching Transitions";
			//turingmachine stops
		}
	}

	this.displayNextTmOperation = function(displayString) {
		operationTextDisplayTapes.text(displayString);
	}

	this.fillTMTapes = function() {
		tapeGraphics.selectAll("text").remove();
		for(var i = 0; i < numberOfTapes; i++){
			//window.alert(i);
			//window.alert(firstViewableTapeElement[i]);
			for(var j = firstViewableTapeElement[i]; j < (firstViewableTapeElement[i] + tapeElementsPerLine); j++){
				tapeGraphics.append('svg:text')
				//var tapeElementLabel = svg.append("text")
					.attr("x", 10 + ((j-firstViewableTapeElement[i])*tapeElementWidth) + (tapeElementWidth/2))
					.attr("y", 15 + ((10+tapeElementHeight)*i) + (tapeElementHeight/2))
					.attr("dy", ".35em")
					.text(tapeList[i][j]);
					//window.alert(i + ":  " + tapeList[i]);
				tapeGraphics.append('svg:text')
					.attr("x", 10 + ((j-firstViewableTapeElement[i])*tapeElementWidth) + (tapeElementWidth/2))
					.attr("y", 20 + ((10+tapeElementHeight)*i) + ((tapeElementHeight)))
					.attr("dy", ".35em")
					.style("font-size", "12px")
					.text(j - 50 - tapeHeadOffset);
			}
		}
	}

    /**
     * Locks the interface, so it is static display
     *
     */
	//Tahiti
     this.lockCanvas = function() {
     	svg.style('pointer-events', 'none');
     	hoverPath.style('pointer-events', 'none');
     	path.style('pointer-events', 'none');
     	hoverMenu.style('pointer-events', 'none');
     	circle.style('pointer-events', 'none');
     	labels.style('pointer-events', 'none');
		d3.select(container).style('pointer-events', 'none');
		if(config.node.label === 'tm')
			hoverMenuTmOperations.style('pointer-events', 'none');

     	trashLabel.style('visibility', 'hidden');
     	clearRect.style('visibility', 'hidden');
     	clearText.style('visibility', 'hidden');
     }

    /**
     * Unlocks the interface, so it is interactive
     *
     */
	//Tahiti
     this.unlockCanvas = function() {
     	svg.style('pointer-events', 'auto');
     	hoverPath.style('pointer-events', 'auto');
     	path.style('pointer-events', 'auto');
     	hoverMenu.style('pointer-events', 'auto');
     	circle.style('pointer-events', 'auto');
     	labels.style('pointer-events', 'auto');
    	d3.select(container).style('pointer-events', 'auto');
		if(config.node.label === 'tm')
			hoverMenuTmOperations.style('pointer-events', 'auto');
		
     	trashLabel.style('visibility', 'visible');
     	clearRect.style('visibility', 'visible');
     	clearText.style('visibility', 'visible');
     }

	this.enterSolveMode = function() {
		solveMode = true
     	trashLabel.style('visibility', 'hidden');
     	clearRect.style('visibility', 'hidden');
     	clearText.style('visibility', 'hidden');
	}

	this.exitSolveMode = function() {
		solveMode = false
     	trashLabel.style('visibility', 'visible');
     	clearRect.style('visibility', 'visible');
     	clearText.style('visibility', 'visible');
	}
}
