/****************************************************************************
 * Gobo v1.0
 * Online Grocery Supermarket Mobile Template by Gambolthemes
 * Copyright 2022 | Gambolthemes
 * @package Gambolthemes
 ****************************************************************************/

/*----------------------------------------------
Index Of Script
------------------------------------------------

:: Save Wishlist
:: Cart Active
:: Star Rating
:: Owl Carousel

------------------------------------------------
Index Of Script
----------------------------------------------*/

/*--- Save Wishlist ---*/
$(document).ready(function() {
	$('.save-icon, .save-button').on('click', function(e) {
		e.preventDefault();
		$(this).toggleClass('saved');
		$(this).children('.save-icon').toggleClass('saved');
	});
});

/*--- Star Rating ---*/
$('.starRating span').click(function(){
  $(this).siblings().removeClass('active');
  $(this).addClass('active');
  $(this).parent().addClass('starRated');
  
  // Added for Demo
  let rating = $(this).index() + 1;
  $('#currentRating').html( "<small>Rating: <b>" + rating + "</b></small>" );
});

/*--- Cart Active ---*/
$(".cart-plus-minus").append('<div class="dec qtybutton">-</div><div class="inc qtybutton">+</div>');
$(".qtybutton").on("click", function () {
	var $button = $(this);
	var oldValue = $button.parent().find("input").val();
	if ($button.text() == "+") {
		var newVal = parseFloat(oldValue) + 1;
	} else {
		// Don't allow decrementing below zero
		if (oldValue > 0) {
			var newVal = parseFloat(oldValue) - 1;
		} else {
			newVal = 0;
		}
	}
	$button.parent().find("input").val(newVal);
}); 

/*--- OWL Carousel ---*/

// Category Slider
$('.category-slider').owlCarousel({
	items:3,
	loop:false,
	margin:10,
	nav:false,
	dots:false,
	responsive:{
		0:{
			items:4
		},
		600:{
			items:6
		},
		800:{
			items:6
		},
		1000:{
			items:6
		},
	}
})

// Best Seller Slider
$('.bestSeller-slider').owlCarousel({
	loop:false,
	items:2,
	margin:10,
	nav:false,
	dots:false,
	responsive:{
		0:{
			items:2
		},
		600:{
			items:3
		},
		1000:{
			items:4
		},
	}
})


// Offer Slider
$('.offer-slider').owlCarousel({
	stagePadding: 50,
    loop:true,
    margin:10,
	nav:false,
	dots:true,
	responsive:{
		0:{
			items:1
		},
		600:{
			items:2
		},
		1000:{
			items:2
		},
	}
})

// featured Items Slider
$('.featuredItems-slider').owlCarousel({
	loop:false,
	items:2,
	margin:10,
	nav:false,
	dots:false,
	responsive:{
		0:{
			items:2
		},
		600:{
			items:3
		},
		1000:{
			items:4
		},
	}
})

// Delivery Date Slider
$('.delivery-date-slider').owlCarousel({
	loop:false,
	items:7,
	margin:10,
	nav:false,
	dots:true,
	responsive:{
		0:{
			items:3
		},
		600:{
			items:6
		},
		1000:{
			items:7
		},
	}
})

// Item Item Slider
$('.imgItem-slider').owlCarousel({
	loop:false,
	items:2,
	margin:10,
	nav:false,
	dots:true,
	responsive:{
		0:{
			items:1
		},
		600:{
			items:3
		},
		1000:{
			items:4
		},
	}
})

// Get Started Slider
$('.getStarted-slider').owlCarousel({
	items:3,
	loop:false,
	margin:10,
	nav:false,
	dots:true,
	responsive:{
		0:{
			items:1
		},
		600:{
			items:1
		},
		800:{
			items:1
		},
		1000:{
			items:1
		},
	}
})
