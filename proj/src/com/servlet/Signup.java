package com.servlet;

import java.sql.*;
import java.text.SimpleDateFormat;
import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

/**
 * Servlet implementation class Signup
 */
@WebServlet("/Signup")
public class Signup extends HttpServlet {
	private static final long serialVersionUID = 1L;
       
    /**
     * @see HttpServlet#HttpServlet()
     */
    public Signup() {
        super();
    }

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		// 한글 인코딩
		request.setCharacterEncoding("UTF-8");
		response.setCharacterEncoding("UTF-8");
		response.setContentType("text/html; charset=UTF-8");
		
		signup(request, response);
	}

	protected void signup(HttpServletRequest request, HttpServletResponse response) {
		try {
			HttpSession session = request.getSession();
			String idcheck = (String)session.getAttribute("idcheck");
			
			PrintWriter out;
			out = response.getWriter();
			
			// 중복체크 한 경우
			if(idcheck != null) {
				// 입력한 것들 받아오기
				String id = request.getParameter("id");
				String pw = request.getParameter("pw");
				String name = request.getParameter("name");
				String gender = request.getParameter("gender");
				String major = request.getParameter("major");
				String grade = request.getParameter("grade");
				
				// session 삭제
				session.invalidate();

				// mssql 연결
				Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
				String connectionUrl = "jdbc:sqlserver://localhost:1433;Database=IP_final;IntegratedSecurity=true";
				Connection conn = DriverManager.getConnection(connectionUrl);
				
				// Query 진행
				PreparedStatement pstmt1 = conn.prepareStatement("SELECT COUNT (*) as cnt from user_data");
				ResultSet rs = pstmt1.executeQuery();
				rs.next();
				int cnt = Integer.parseInt(rs.getString("cnt"));
				
				// Query 진행
				PreparedStatement pstmt2 = conn.prepareStatement("insert into user_data values (?, ?, ?, ?, ?, ?, ?)");

				pstmt2.setInt(1, cnt + 1);
				pstmt2.setString(2, id);
				pstmt2.setString(3, pw);
				pstmt2.setString(4, name);
				pstmt2.setString(5, gender);
				pstmt2.setString(6, major);
				pstmt2.setString(7, grade);
				pstmt2.executeUpdate();
				
				SimpleDateFormat sdt = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
				String cur_time = sdt.format(new Date(System.currentTimeMillis()));
				System.out.println(id + " signup " + cur_time);
				
				// 경고창 띄우고 첫 화면으로 이동
				out.println("<script>");
				out.println("alert('회원가입이 완료되었습니다.');");
				out.println("location.href = '.'"); 
				out.println("</script>"); 
				out.close();
				
				rs.close();
				pstmt1.close();
				pstmt2.close();
				conn.close();
			} 
			// 중복체크 안한 경우
			else {
				// 경고창 띄우고 첫 화면으로 이동
				out.println("<script>");
				out.println("alert('id 중복 검사를 해주세요.');");
				out.println("location.href = 'Signup.jsp'"); 
				out.println("</script>"); 
				out.close();
			}
		} catch (IOException e) {
			e.printStackTrace();
		} catch (ClassNotFoundException e) {
			e.printStackTrace();
		} catch (SQLException e) {
			e.printStackTrace();
		}
	}
}
