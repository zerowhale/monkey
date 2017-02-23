function Card(id, key, deck) {
    this.id = id;
    this.key = key;
    if (!Card.template)
        Card.template = $("#game .templates .card").clone();

    this.display = Card.template.clone();
    this.display.find(".back").attr("src", "/content/pandemic/img/cards/" + deck + "/back.png");

    this.setFace = function (id, key) {
        var obj = this;
        this.id = id;
        this.key = key;
        if (id !== undefined && id !== null) {
            this.display.attr("data-id", id);
        }
        this.display.on("click", function (e) {
            e.stopPropagation();
            Game.centerCard(obj);
        })

        this.display.find(".front").attr("src", "/content/pandemic/img/cards/" + deck + "/" + key + ".png");
    }

    this.flip = function () {
        this.display.toggleClass("flipped");
    }

    this.clone = function () {
        var card = new Card(this.id, this.key, deck);
        if (this.display.hasClass("flipped"))
            card.flip();
        return card;
    }

    this.center = function () {
        this.display.addClass("centered");
        this.display.css({
            top: "",
            left: ""
        });
    }

    {
        this.setFace(id, key);
    }    
}
Card.template = null;


var CardIds = {
    Epidemic: 100
}