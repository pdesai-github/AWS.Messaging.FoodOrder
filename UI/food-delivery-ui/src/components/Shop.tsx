import axios from "axios";
import { useEffect, useState } from "react";
import { FoodOrderMessage } from "../models/FoodOrderMessage";
import './shop.css'

interface ShopProps {
    shopId: string
}
const Shop: React.FC<ShopProps> = ({ shopId }) => {
   
    const [orders, setOrders] = useState<FoodOrderMessage[]>([]);

    const getOrders = async () => {
        const res = await axios.get("https://localhost:7038/api/Shop?shopId=" + shopId);
        setOrders(res.data);
    };

    const deleteOrder = async (receiptHandle: string) => {
        await axios.delete(`https://localhost:7038/api/Shop?shopId=${encodeURIComponent(shopId)}`, {
            data: { receiptHandle }
        });
        setOrders(prev => prev.filter(o => o.receiptHandle !== receiptHandle));
    };

    useEffect(() => {
        getOrders();
    }, [shopId]);

    return (<>
        <div className="d-flex align-items-center gap-2 main">
            <h3>{shopId}</h3>
            <button className="btn btn-sm btn-outline-primary" onClick={getOrders}>Refresh</button>
        </div>
        <div>
            <ul className="list-group mt-2">
                {orders.map((order) => (
                    <li key={order.messageId} className="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <strong>Order ID:</strong> {order.messageId}<br />
                            <span>{order.body}</span>
                        </div>
                        <button className="btn btn-sm btn-outline-danger" onClick={() => deleteOrder(order.receiptHandle!)}>Delete</button>
                    </li>
                ))}
            </ul>
        </div>

    </>)
}

export default Shop;