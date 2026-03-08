import { useEffect, useState } from "react"
import { FoodOrderMessage } from "../models/FoodOrderMessage"
import axios from "axios"

const DeliveryDLQ: React.FC = () => {

    const [orders, setOrders] = useState<FoodOrderMessage[]>([])

    const getDLQOrders = async () => {
        const res = await axios.get("https://localhost:7185/api/deliveryorder/dlq/messages")
        const orders_from_api: FoodOrderMessage[] = res.data;
        setOrders(orders_from_api)
    }

    const redrive = async ()=> {
        await axios.post("https://localhost:7185/api/deliveryorder/dlq/redrive")
        alert("Messages redrived successfully")
    }

    useEffect(() => {
        getDLQOrders()
    }, [])

    return (
        <div>
            <h3>Delivery DLQ</h3>
            <div>
                <button className="btn btn-sm btn-primary" onClick={() => getDLQOrders()}>Refresh</button>
            </div>
            <hr />
            <ul>
                {
                    orders.map((order, index) => ( <li key={order.messageId} className="list-group-item d-flex justify-content-between align-items-center">
                        <span>
                            <strong>Message ID:</strong> {order.messageId} &nbsp;|&nbsp;
                            <strong>Body:</strong> {order.body}
                        </span>
                        <button
                            className="btn btn-sm btn-danger"
                            onClick={() => redrive()}
                        >Redrive</button>
                    </li>))
                }
            </ul>
        </div>
    )
}

export default DeliveryDLQ;