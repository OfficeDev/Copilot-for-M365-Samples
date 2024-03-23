import axios from 'axios';

import {
  ACCESS_TOKEN_API,
  DB2_PASSWORD,
  DB2_USER,
  DB2HOST,
  EXEC_SQL_JOB,
  IBM_TENANT_ID,
} from '../constants/constants';

let fmtdata;

const getDB2AccessToken = async () => {
  const options = {
    method: "post",
    headers: {
      "content-type": "application/json",
      "x-deployment-id": IBM_TENANT_ID,
    },
    url: `https://${DB2HOST}${ACCESS_TOKEN_API}`,
    data: {
      userid: DB2_USER,
      password: DB2_PASSWORD,
    },
  };

  try {
    const res = await axios(options);
    return res.data.token;
  } catch (error) {
    console.log("Error getting token", error);
  }
};

export const createJSONFromData = (rows, columns) => {
  if (!rows || !columns) return;
  const items = [];
  const jsonObject = [];
  for (let index = 0; index < rows.length; index++) {
    const row = rows[index];
    const jsonObject = {};
    for (let i = 0; i < columns.length; i++) {
      jsonObject[columns[i]] = row[i];
    }

    items.push(jsonObject);
  }

  return items;
};

export const checkJobExecution = async (jobId) => {
  const accessToken = await getDB2AccessToken();
  try {
    const options = {
      method: "get",
      headers: {
        "content-type": "application/json",
        "x-deployment-id": IBM_TENANT_ID,
        authorization: `Bearer ${accessToken}`,
      },
      url: `https://${DB2HOST}${EXEC_SQL_JOB}/${jobId}`,
    };
    const res = await axios(options);
    return res.data;
  } catch (error) {
    console.log("Error checking job execution", error);
  }
};

export const submitDb2Job = async (query) => {
  try {
    if (!query) {
      console.log("[getData ] error:", "query to execute is undefined");
      return;
    }
    const accessToken = await getDB2AccessToken();
    const queryDef = {
      commands: query,
      limit: 1000,
      separator: ";",
      stop_on_error: "no",
    };
    const options = {
      method: "post",
      headers: {
        "content-type": "application/json",
        "x-deployment-id": IBM_TENANT_ID,
        authorization: `Bearer ${accessToken}`,
      },
      url: `https://${DB2HOST}${EXEC_SQL_JOB}`,
      data: queryDef,
    };

    const res = await axios(options);
    return res.data;
  } catch (error) {
    console.log("Error submitting job", error);
  }
};

const executeQuery = async (query: string) => {
  try {
    const jobInfo = await submitDb2Job(query);
    if (jobInfo) {
      const { id } = jobInfo;
      while (true) {
        const jobdata = await checkJobExecution(id);
        const { status, results } = jobdata || {};
        const { columns = [], rows = [] } = results?.length ? results[0] : {};
        if (status === "completed" && !jobdata.results.length) {
          return undefined;
        }
        switch (status) {
          case "completed":
            fmtdata = createJSONFromData(rows, columns);
            return fmtdata;
          case "failed":
            console.log("Job failed", jobdata);
            return undefined;
        }
      }
    }
  } catch (error) {
    console.log("Error executing query", error);

    throw error;
  }
};

export const getOrders = async (
  searchOrdersQuery: string,
  clientQuery: string,
  orderStatusQuery: string,
  orderDateQuery: string,
  orderAmountQuery: string
): Promise<any> => {
  let whereClause = searchOrdersQuery.trimEnd().length
    ? `TO_CHAR(CUST_ORD.ORD_NBR) like '${searchOrdersQuery}%'`
    : "CUST_ORD.ORD_NBR > 0";
  if (clientQuery) {
    whereClause += `  and C2.CUST_FRST_NAME like '${clientQuery}%' or C2.CUST_LAST_NAME like '${clientQuery}%'`;
  }
  if (orderStatusQuery) {
    whereClause += ` and CUST_ORD.ORD_STAT = ${orderStatusQuery}`;
  }
  if (orderDateQuery) {
    whereClause += ` and CUST_ORD.ORD_DATE = ${orderDateQuery}`;
  }
  if (orderAmountQuery) {
    whereClause += ` and CUST_ORD.ORD_TOT_COST = ${orderAmountQuery}`;
  }

  const sql = `select * from CUST_ORD left join DWH00649.CUST C2 on C2.CUST_CODE = CUST_ORD.CUST_CODE  where ${whereClause} order by C2.CUST_CODE`;

  const data = await executeQuery(sql);
  return data;
};


export const updateOrderStatus = async (orderId:string, orderStatus:string) => {
   
  const query = `UPDATE CUST_ORD SET ORD_STAT = '${orderStatus}' WHERE ORD_NBR = ${orderId}`;
  const data = await executeQuery(query);
  
};
 
export  const getOrder = async (orderId:string) => {
  const query = `select * from CUST_ORD left join DWH00649.CUST C2 on C2.CUST_CODE = CUST_ORD.CUST_CODE  where ORD_NBR = ${orderId} `;
  const data = await executeQuery(query);
  return data;
}

