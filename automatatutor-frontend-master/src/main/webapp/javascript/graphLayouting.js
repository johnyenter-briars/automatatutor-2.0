/*
 * Module for calling graph layouting algorithms
 * use public functions graphLayouting.'function'('params')
 * for use in d3 force simulations
 * Attention: These functions must change 
 * node positions or velocity or they will not work properly
 * !! For usage in future problems make sure to include a script
 * reference to this script in the creation-html !!
 */
var graphLayouting = (function() {
    //list of all radial angles for Radial force
    var angles = [];
    /**
     * Graph layouting with spring embedder
     * Nodes are connected with springs -> attracting force
     * All nonadjecent nodes repell each other
     */
    function graphLayoutingEades(alpha, nodes, links) 
    {
        //force variable for moving nodes 
        var layoutForce = {
            x: 0,
            y: 0
        };
        var ForcesOnNodes = [];
        //constants
        var c_rep = 1000,
            c_spring = 10,
            spring_length = 150, //ideal distance between nodes in pixels
            delta = 1;
        //forces applided to each node	
        var springForce = {
                x: 0,
                y: 0
            }, //initialize?
            repellingForce = {
                x: 0,
                y: 0
            };
        var isadjecent = false;
        //new position of the node
        var newNodePosx = 0;
        var newNodePosy = 0;
        //helper variables for double repellent force
        var doubleEdges = [];
        var double = false;
        for (var i = 0; i < nodes.length; i++) {
            //check for adjacent nodes
            for (var j = 0; j < links.length; j++) {
                //var sourceNode = links[j].source,
                //	targetNode = links[j].target;
                if (links[j].source.id != links[j].target.id) {
                    //console.log(links[j].source.id + " and " + links[j].target.id + " are diffrent!");
                    if (links[j].source.id == nodes[i].id) {
                        var euclidProdSource = Math.sqrt(Math.pow((links[j].source.x - links[j].target.x), 2) + Math.pow((links[j].source.y - links[j].target.y), 2));
                        var unitVectorSource = {
                            x: (links[j].target.x - links[j].source.x) / euclidProdSource,
                            y: (links[j].target.y - links[j].source.y) / euclidProdSource
                        };
                        springForce.x += c_spring * Math.log(euclidProdSource / spring_length) * unitVectorSource.x;
                        springForce.y += c_spring * Math.log(euclidProdSource / spring_length) * unitVectorSource.y;
                    } else if (links[j].target.id == nodes[i].id) {
                        var euclidProdTarget = Math.sqrt(Math.pow((links[j].source.x - links[j].target.x), 2) + Math.pow((links[j].source.y - links[j].target.y), 2));
                        var unitVectorTarget = {
                            x: (links[j].source.x - links[j].target.x) / euclidProdTarget,
                            y: (links[j].source.y - links[j].target.y) / euclidProdTarget
                        };
                        springForce.x += c_spring * Math.log(euclidProdTarget / spring_length) * unitVectorTarget.x;
                        springForce.y += c_spring * Math.log(euclidProdTarget / spring_length) * unitVectorTarget.y;
                    } else {}
                }
            }
            /*check for nonadjecent nodes for repellent force
             *
             *k denotes target for pair (i,k) in repellent force
             */
            for (var k = 0; k < nodes.length; k++) {
                var srsNode = nodes[i],
                    trgNode = nodes[k];
                if (nodes[k].id != nodes[i].id) {
                    //check for existing edges: l denotes node with an edge
                    for (var l = 0; l < links.length; l++) {
                        //calculate force only for nonadjecent nodes
                        if (links[l].source.id == nodes[i].id && links[l].target.id == nodes[k].id) {
                            isadjecent = true;
                            doubleEdges.push({
                                Source: links[l].target.id,
                                Target: links[l].source.id
                            });
                        }
                    }
                    for (var e = 0; e < doubleEdges.length; e++) {
                        if (nodes[i].id == doubleEdges[e].Source && nodes[k].id == doubleEdges[e].Target) {
                            double = true;
                        }
                    }
                    if (!double && !isadjecent) {
                        var euclidProd = Math.sqrt(Math.pow((nodes[i].x - nodes[k].x), 2) + Math.pow((nodes[i].y - nodes[k].y), 2));
                        var unitVector = {
                            x: (nodes[i].x - nodes[k].x) / euclidProd,
                            y: (nodes[i].y - nodes[k].y) / euclidProd
                        };
                        repellingForce.x += c_rep / Math.pow(euclidProd, 2) * unitVector.x;
                        repellingForce.y += c_rep / Math.pow(euclidProd, 2) * unitVector.y;
                    }
                }
                isadjecent = false;
                double = false;
            }
            //apply both forces onto node
            layoutForce.x += springForce.x + repellingForce.x,
                layoutForce.y += springForce.y + repellingForce.y;
            ForcesOnNodes.push(layoutForce);
            newNodePosx = nodes[i].x + delta * layoutForce.x;
            newNodePosy = nodes[i].y + delta * layoutForce.y;
            nodes[i].x = newNodePosx;
            nodes[i].y = newNodePosy;
            //reset forces
            springForce.x = 0;
            springForce.y = 0;
            repellingForce.x = 0;
            repellingForce.y = 0;
            layoutForce.x = 0;
            layoutForce.y = 0;
            newNodePosx = 0;
            newNodePosy = 0;
        }
        //helper function for euclidean product calculation
        function euclidProd(u, v) {
            var result;
            result = Math.sqrt(Math.pow((u.x - v.x), 2) + Math.pow((u.y - v.y), 2));
            return result;
        }
        //helper function for unit vector calculation
        function unitVector(u, v, euPr) {
            var result = {
                x,
                y
            };
            result.x = (v.x - u.x) / euPr;
            result.y = (v.y - u.y) / euPr;
            return result;
        }
    }
    /*
     * Graph layouting with tweaked Eades
     * All Nodes repell each other
     * Forces weaken over time like temperature cooling -> simulated annealing
     */
    function graphLayoutingFruRein(alpha, nodes, links, width, height) 
    {
        var c = 1,
            area = width * height,
            numofNodes = nodes.length;
        /*var dispN = {x: 0, y: 0},
        	dispE = {v,u};*/
        var k = c * Math.sqrt(area / numofNodes);
        var t = width * alpha;
        var dispList = [];
        var buffer = 20;
        //repelling force between all node-pairs
        for (var v = 0; v < nodes.length; v++) {
            var dispN = {
                x: 0,
                y: 0,
                id: 0
            };
            for (var u = 0; u < nodes.length; u++) {
                if (nodes[v].id != nodes[u].id) {
                    var diffN = {
                        x: nodes[v].x - nodes[u].x,
                        y: nodes[v].y - nodes[u].y
                    };
                    var diffEuclidN = Math.sqrt(Math.pow(diffN.x, 2) + Math.pow(diffN.y, 2));
                    //prevention of divide by zero
                    if (diffEuclidN == 0) diffEuclidN = 0.1;
                    dispN.x += (diffN.x / diffEuclidN) * repForce(diffEuclidN);
                    dispN.y += (diffN.y / diffEuclidN) * repForce(diffEuclidN);
                    var dispEuclidN = Math.sqrt(Math.pow(dispN.x, 2) + Math.pow(dispN.y, 2));
                    //prevention of divide by zero
                    if (dispEuclidN == 0) dispEuclidN = 0.1;
                    //simulated annealing step & stay within boundaries of content
                    var posX = nodes[v].x + (dispN.x / dispEuclidN) * Math.min(Math.abs(dispN.x), t);
                    var posY = nodes[v].y + (dispN.y / dispEuclidN) * Math.min(Math.abs(dispN.y), t);
                }
            }
            dispN.id = v;
            dispList.push(dispN);
        }
        //spring force between adjecent nodes
        for (var e = 0; e < links.length; e++) {
            //search only non-reflexive edges
            if (links[e].source.id != links[e].target.id) {
                var dispE = {
                    n: {
                        x: 0,
                        y: 0
                    },
                    u: {
                        x: 0,
                        y: 0
                    }
                };
                var diffE = {
                    x: links[e].source.x - links[e].target.x,
                    y: links[e].source.y - links[e].target.y
                };
                var diffEuclidE = Math.sqrt(Math.pow(diffE.x, 2) + Math.pow(diffE.y, 2));
                //prevention of divide by zero
                if (diffEuclidE == 0) diffEuclidE = 0.1;
                dispE.n = {
                    x: dispE.n.x - (diffE.x / diffEuclidE) * attrForce(diffEuclidE),
                    y: dispE.n.y - (diffE.y / diffEuclidE) * attrForce(diffEuclidE)
                };
                dispE.u = {
                    x: dispE.u.x + (diffE.x / diffEuclidE) * attrForce(diffEuclidE),
                    y: dispE.u.y + (diffE.y / diffEuclidE) * attrForce(diffEuclidE)
                };
                //simulated annealing step & stay wihin boundaries of content
                for (var m = 0; m < dispList.length; m++) {
                    if (dispList[m].id == links[e].source.id) {
                        dispList[m].x += dispE.n.x;
                        dispList[m].y += dispE.n.y;
                    } else if (dispList[m].id == links[e].target.id) {
                        dispList[m].x += dispE.u.x;
                        dispList[m].y += dispE.u.y;
                    }
                }
            }
        }
        //simulated annealing step & stay within boundaries of content
        for (var n = 0; n < nodes.length; n++) {
            var dispEuclidN = Math.sqrt(Math.pow(dispList[n].x, 2) + Math.pow(dispList[n].y, 2));
            //prevention of divide by zero
            if (dispEuclidN == 0) dispEuclidN = 0.1;
            nodes[n].x += (dispList[n].x / dispEuclidN) * Math.min(Math.abs(dispList[n].x), t);
            nodes[n].y += (dispList[n].y / dispEuclidN) * Math.min(Math.abs(dispList[n].y), t);
        }
        //t = cool(t);
        dispList = [];
        //helper function for repelling force
        function repForce(z) {
            return (k * k) / (z * 1000) * (alpha * alpha);
        }
        //helper function for attractive force
        function attrForce(z) {
            return (z * z) / (k * 350) * (alpha);
        }
        /*
         * helper function for simulated annealing
         * Temerature t decays in inverse linear fashion
         */
        function cool(t) {
            if (t > 0) {
                //m between 10 and 30 seems good
                var m = 20;
                t -= m;
                return t;
            } else {
                return 0;
            }
        }
        //helper function for euclidean product calculation
        function euclidProd(u, v) {
            var result;
            result = Math.sqrt(Math.pow((u.x - v.x), 2) + Math.pow((u.y - v.y), 2));
            return result;
        }
        /*
		//helper function for unit vector calculation
		function unitVector(u,v, euPr)
		{
			var result = {x,y};
			result.x = (v.x - u.x)/euPr;
	  		result.y = (v.y - u.y)/euPr;
	  		return result;
		}
		*/
    }
    /*
     * new force that sorts nodes on the circle around specified center
     */
    function graphLayoutingRadial(alpha, nodes, links, center, radius) 
    {
        var numNodes = nodes.length;
        //starting position for better view
        if (numNodes == 1) {
            nodes[0].x = center.x;
            nodes[0].y = center.y;
            return
        }
        var TWO_PI = 2 * Math.PI;
        var angle = TWO_PI / numNodes;
        for (var a = 0, i = 0; a < TWO_PI, i < numNodes; a += angle, i++) {
            var sx = center.x + Math.cos(a) * radius;
            var sy = center.y - Math.sin(a) * radius;
            nodes[i].x = sx;
            nodes[i].y = sy;
            var newnode = true;
            for (var j = 0; j < angles.length; j++) {
                if (angles[j].id == nodes[i].id) {
                    angles[j].alpha = a;
                    newnode = false;
                }
            }
            if (newnode) {
                angles.push({
                    id: nodes[i].id,
                    alpha: a
                });
            }
        }
    }

    function graphLayoutingSpringEmbedder(alpha, nodes, links, width, height) 
    {
        var c_spring = 10,
            spring_length = 200, //ideal distance between nodes in pixels
            k = 1 * Math.sqrt(width * height);
        //forces applided to each node	
        var springForce = {
                x: 0,
                y: 0
            }, //initialize?
            repellingForce = {
                x: 0,
                y: 0
            },
            resultingForce = [];
        for (var i = 0; i < nodes.length; i++) {
            //check for adjacent nodes
            for (var j = 0; j < links.length; j++) {
                if (links[j].source.id != links[j].target.id) {
                    if (links[j].source.id == nodes[i].id) {
                        var euclidProdSource = Math.sqrt(Math.pow((links[j].source.x - links[j].target.x), 2) + Math.pow((links[j].source.y - links[j].target.y), 2));
                        var unitVectorSource = {
                            x: (links[j].target.x - links[j].source.x) / euclidProdSource,
                            y: (links[j].target.y - links[j].source.y) / euclidProdSource
                        };
                        springForce.x += c_spring * Math.log(euclidProdSource / spring_length) * unitVectorSource.x;
                        springForce.y += c_spring * Math.log(euclidProdSource / spring_length) * unitVectorSource.y;
                    } else if (links[j].target.id == nodes[i].id) {
                        var euclidProdTarget = Math.sqrt(Math.pow((links[j].source.x - links[j].target.x), 2) + Math.pow((links[j].source.y - links[j].target.y), 2));
                        var unitVectorTarget = {
                            x: (links[j].source.x - links[j].target.x) / euclidProdTarget,
                            y: (links[j].source.y - links[j].target.y) / euclidProdTarget
                        };
                        springForce.x += c_spring * Math.log(euclidProdTarget / spring_length) * unitVectorTarget.x;
                        springForce.y += c_spring * Math.log(euclidProdTarget / spring_length) * unitVectorTarget.y;
                    } else {}
                }
            }
            //repelling force between all other nodes	
            for (var u = 0; u < nodes.length; u++) {
                var dispN = {
                    x: 0,
                    y: 0
                };
                if (nodes[i].id != nodes[u].id) {
                    var diffN = {
                        x: nodes[i].x - nodes[u].x,
                        y: nodes[i].y - nodes[u].y
                    };
                    var diffEuclidN = Math.sqrt(Math.pow(diffN.x, 2) + Math.pow(diffN.y, 2));
                    //prevention of divide by zero
                    if (diffEuclidN == 0) diffEuclidN = 0.1;
                    dispN.x += (diffN.x / diffEuclidN) * repForce(diffEuclidN);
                    dispN.y += (diffN.y / diffEuclidN) * repForce(diffEuclidN);
                    var dispEuclidN = Math.sqrt(Math.pow(dispN.x, 2) + Math.pow(dispN.y, 2));
                    //prevention of divide by zero
                    if (dispEuclidN == 0) dispEuclidN = 0.1;
                    //simulated annealing step & stay within boundaries of content
                    var testX = (dispN.x / dispEuclidN) * Math.abs(dispN.x),
                        testY = (dispN.y / dispEuclidN) * Math.abs(dispN.y);
                    if (Math.abs(testX) >= 5) repellingForce.x += testX;
                    if (Math.abs(testY) >= 5) repellingForce.y += testY;
                }
            }
            var temp = {
                x: (springForce.x + repellingForce.x),
                y: (springForce.y + repellingForce.y),
                id: i
            }
            resultingForce.push(temp);
            springForce = {
                x: 0,
                y: 0
            };
            repellingForce = {
                x: 0,
                y: 0
            };
        }
        for (var m = 0; m < resultingForce.length; m++) {
            for (var n = 0; n < nodes.length; n++) {
                if (resultingForce[m].id == nodes[n].id) {
                    nodes[n].x += resultingForce[m].x;
                    nodes[n].y += resultingForce[m].y;
                }
            }
        }
        //helper function for repelling force
        function repForce(z) {
            var buffer = 500; //weaken rep force because of small canvas
            return (k * k) / (z * buffer) * (alpha * alpha);
        }
    }

    function graphLayoutingSortRandom(alpha, nodes, links, width, height) 
    {
        var m = 200,
            buffer = 150;
        //set random location  for nodes
        for (var i = 0; i < nodes.length; i++) {
            nodes[i].x = d3.randomUniform(0 + buffer, width - buffer)();
            nodes[i].y = d3.randomUniform(0 + buffer, height - buffer)();
        }
        //force loop
        for (var n = 0; n < m; n++) {
            graphLayoutingFruRein(alpha, nodes, links, width, height);
        }
    }
    //return public function for call in interface.js
    return {
        eades: function(alpha, nodes, links) {
            return graphLayoutingEades(alpha, nodes, links);
        },
        frurein: function(alpha, nodes, links, width, height) {
            return graphLayoutingFruRein(alpha, nodes, links, width, height);
        },
        radial: function(alpha, nodes, links, center, radius) {
            return graphLayoutingRadial(alpha, nodes, links, center, radius);
        },
        radial_angles: function() {
            return angles;
        },
        frureinvar: function(alpha, nodes, links) {
            return FruReinVar(alpha, nodes, links);
        },
        sort_random: function(alpha, nodes, links, width, height) {
            return graphLayoutingSortRandom(alpha, nodes, links, width, height);
        },
        springembedder: function(alpha, nodes, links, width, height) {
            return graphLayoutingSpringEmbedder(alpha, nodes, links, width, height);
        }
    };
})();