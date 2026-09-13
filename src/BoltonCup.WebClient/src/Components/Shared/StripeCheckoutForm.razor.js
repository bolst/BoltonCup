let stripe;
let elements;

export function initializePaymentElement (publishableKey, clientSecret) {
    stripe = Stripe(publishableKey);

    const appearance = { theme: 'stripe' };
    elements = stripe.elements({ clientSecret, appearance });

    const paymentElementOptions = {
        layout: 'tabs'
    }
    const paymentElement = elements.create("payment", paymentElementOptions);
    paymentElement.mount("#payment-element");
}

export async function confirmPayment() {

    const { error, paymentIntent } = await stripe.confirmPayment({
        elements: elements,
        redirect: "if_required"
    });

    if (error) {
        return { success: false, error: error.message };
    } else if (paymentIntent && paymentIntent.status === "succeeded") {
        return { success: true };
    } else {
        return { success: false, error: "An unexpected error occurred." };
    }
}
